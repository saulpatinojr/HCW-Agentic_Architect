using System.IO.Compression;

namespace WorkspaceManager.Services;

public sealed class WorkspacePackUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly PackManifestService _packManifestService;
    private readonly WorkspacePackCatalogService _catalogService;

    public WorkspacePackUpdateService()
        : this(new HttpClient(), new PackManifestService(), new WorkspacePackCatalogService())
    {
    }

    public WorkspacePackUpdateService(HttpClient httpClient, PackManifestService packManifestService, WorkspacePackCatalogService catalogService)
    {
        _httpClient = httpClient;
        _packManifestService = packManifestService;
        _catalogService = catalogService;
    }

    public async Task<WorkspacePackUpdateResult> UpdateAsync(string repoRootPath, AgentViewModel pack, CancellationToken cancellationToken = default)
    {
        var logs = new List<string>();
        var catalogEntry = _catalogService.LoadCatalog(repoRootPath)
            .FirstOrDefault(p => p.Id.Equals(pack.DirectoryName, StringComparison.OrdinalIgnoreCase));

        string sourceRepository = string.IsNullOrWhiteSpace(pack.SourceRepository)
            ? catalogEntry?.SourceRepository ?? string.Empty
            : pack.SourceRepository;
        string sourceBranch = string.IsNullOrWhiteSpace(pack.SourceBranch)
            ? catalogEntry?.SourceBranch ?? "main"
            : pack.SourceBranch;
        string sourcePath = string.IsNullOrWhiteSpace(pack.SourcePath)
            ? catalogEntry?.SourcePath ?? $"workspace-config/agents/{pack.DirectoryName}"
            : pack.SourcePath;

        if (string.IsNullOrWhiteSpace(sourceRepository) || string.IsNullOrWhiteSpace(sourcePath))
        {
            return new WorkspacePackUpdateResult(false, logs)
            {
                Message = "Pack source metadata is missing."
            };
        }

        logs.Add($"[*] Downloading pack update for {pack.DirectoryName} from {sourceRepository}@{sourceBranch}.");

        string repoZipUrl = BuildArchiveUrl(sourceRepository, sourceBranch);
        string tempRoot = Path.Combine(Path.GetTempPath(), "workspace-pack-update", Guid.NewGuid().ToString("N"));
        string downloadPath = Path.Combine(tempRoot, "repo.zip");
        string extractedPackPath = Path.Combine(tempRoot, "pack");
        Directory.CreateDirectory(extractedPackPath);

        try
        {
            Directory.CreateDirectory(tempRoot);
            await using (var stream = await _httpClient.GetStreamAsync(repoZipUrl, cancellationToken))
            await using (var file = File.Create(downloadPath))
            {
                await stream.CopyToAsync(file, cancellationToken);
            }

            await ExtractPackFromArchiveAsync(downloadPath, sourceRepository, sourceBranch, sourcePath, extractedPackPath, cancellationToken);

            var validation = _packManifestService.ValidatePack(extractedPackPath);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    logs.Add($"[-] {error}");
                }

                return new WorkspacePackUpdateResult(false, logs)
                {
                    Message = "Downloaded pack failed validation."
                };
            }

            string targetDir = Path.Combine(repoRootPath, "workspace-config", "agents", pack.DirectoryName);
            string backupDir = targetDir + ".backup-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string stagingDir = extractedPackPath;
            bool movedToBackup = false;

            try
            {
                if (Directory.Exists(targetDir))
                {
                    Directory.Move(targetDir, backupDir);
                    movedToBackup = true;
                }

                Directory.Move(stagingDir, targetDir);

                if (Directory.Exists(backupDir))
                {
                    Directory.Delete(backupDir, true);
                }
            }
            catch
            {
                if (movedToBackup && Directory.Exists(backupDir) && !Directory.Exists(targetDir))
                {
                    Directory.Move(backupDir, targetDir);
                }

                throw;
            }

            logs.Add($"[+] Updated {pack.DirectoryName} to version {validation.Manifest?.Version ?? pack.Version}.");
            return new WorkspacePackUpdateResult(true, logs)
            {
                Message = "Pack updated successfully.",
                UpdatedVersion = validation.Manifest?.Version ?? pack.Version
            };
        }
        catch (Exception ex)
        {
            logs.Add($"[-] Pack update failed: {ex.Message}");
            return new WorkspacePackUpdateResult(false, logs)
            {
                Message = ex.Message
            };
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                try
                {
                    Directory.Delete(tempRoot, true);
                }
                catch
                {
                }
            }
        }
    }

    private static string BuildArchiveUrl(string sourceRepository, string sourceBranch)
    {
        string[] segments = sourceRepository.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 2)
        {
            throw new InvalidOperationException($"Invalid source repository '{sourceRepository}'.");
        }

        return $"https://codeload.github.com/{segments[0]}/{segments[1]}/zip/refs/heads/{sourceBranch}";
    }

    private static async Task ExtractPackFromArchiveAsync(
        string archivePath,
        string sourceRepository,
        string sourceBranch,
        string sourcePath,
        string destinationRoot,
        CancellationToken cancellationToken)
    {
        string[] segments = sourceRepository.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string repoName = segments[1];
        string archiveRootPrefix = $"{repoName}-{sourceBranch}/";
        string normalizedSourcePath = sourcePath.Replace('\\', '/').Trim('/');
        string packPrefix = $"{archiveRootPrefix}{normalizedSourcePath}/";

        await using var archiveStream = File.OpenRead(archivePath);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }

            if (!entry.FullName.StartsWith(packPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string relative = entry.FullName[packPrefix.Length..];
            string targetPath = Path.Combine(destinationRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await using var entryStream = entry.Open();
            await using var file = File.Create(targetPath);
            await entryStream.CopyToAsync(file, cancellationToken);
        }
    }
}

public sealed class WorkspacePackUpdateResult
{
    public WorkspacePackUpdateResult(bool succeeded, IReadOnlyList<string> logs)
    {
        Succeeded = succeeded;
        Logs = logs;
    }

    public bool Succeeded { get; }
    public IReadOnlyList<string> Logs { get; }
    public string Message { get; set; } = string.Empty;
    public string UpdatedVersion { get; set; } = string.Empty;
}
