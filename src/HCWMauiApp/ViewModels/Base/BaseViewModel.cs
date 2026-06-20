using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HCWMauiApp.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels providing common functionality.
/// Uses MVVM Community Toolkit for property change notifications.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Indicates if the view is currently initializing.
    /// </summary>
    public bool IsInitialized { get; set; }

    /// <summary>
    /// Called when the view is navigated to. Override in derived classes.
    /// </summary>
    public virtual Task OnNavigatedTo()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the view is navigated from. Override in derived classes.
    /// </summary>
    public virtual Task OnNavigatedFrom()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Shows an error message and logs it.
    /// </summary>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        System.Diagnostics.Debug.WriteLine($"[ERROR] {message}");
    }

    /// <summary>
    /// Clears the current error message.
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = null;
    }
}
