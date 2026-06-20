namespace HCWMauiApp.Models;

/// <summary>
/// Example domain model.
/// Models should be simple data structures with little to no business logic.
/// </summary>
public class ExampleModel
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this item is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
