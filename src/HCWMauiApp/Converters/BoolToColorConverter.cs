using System.Globalization;

namespace HCWMauiApp.Converters;

/// <summary>
/// Example converter: Converts boolean to color.
/// MAUI data binding converter template.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the color when value is true.
    /// </summary>
    public Color TrueColor { get; set; } = Colors.Green;

    /// <summary>
    /// Gets or sets the color when value is false.
    /// </summary>
    public Color FalseColor { get; set; } = Colors.Red;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueColor : FalseColor;
        }

        return FalseColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
