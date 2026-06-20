using System.ComponentModel;

namespace HCWMauiApp.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels providing common functionality.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    private string _title = string.Empty;
    private string? _errorMessage;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Indicates whether the view is currently loading.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Gets or sets the visible title for the view.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Gets or sets the current error message.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

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

    protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
