using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HCWMauiApp.Utilities;

/// <summary>
/// Extension methods for common operations.
/// Add more helpers as needed for your application.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Adds a range of items to an ObservableCollection.
    /// </summary>
    public static void AddRange<T>(
        this ObservableCollection<T> collection,
        IEnumerable<T> items)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Removes a range of items from an ObservableCollection.
    /// </summary>
    public static void RemoveRange<T>(
        this ObservableCollection<T> collection,
        IEnumerable<T> items)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items.ToList())
        {
            collection.Remove(item);
        }
    }
}
