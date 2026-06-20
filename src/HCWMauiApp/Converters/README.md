# Converters

This folder contains **XAML value converters** for data binding transformations.

## Purpose

Converters transform data between View and ViewModel in XAML bindings.

## Examples

- Boolean to Color (status indicator)
- String to uppercase (display formatting)
- DateTime to formatted string
- Boolean inverse (show/hide conditions)
- List count to visibility

## Structure

One converter per file, named `[From]To[To]Converter.cs`

```csharp
using System.Globalization;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.Green;
    public Color FalseColor { get; set; } = Colors.Red;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? TrueColor : FalseColor;
        return FalseColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

## Usage in XAML

```xml
<ResourceDictionary>
    <converters:BoolToColorConverter 
        x:Key="BoolToColorConverter"
        TrueColor="Green"
        FalseColor="Red" />
</ResourceDictionary>

<BoxView 
    Color="{Binding IsActive, Converter={StaticResource BoolToColorConverter}}" />
```

## Common Converters

- `InverseBoolConverter` - Inverts boolean values
- `BoolToVisibilityConverter` - Boolean to visibility state
- `StringNullOrEmptyConverter` - Check for null/empty strings
- `DateTimeToStringConverter` - Format dates
- `DecimalToStringConverter` - Format numbers with currency
