# Resources

This folder contains **app-wide resources** (images, fonts, colors, styles).

## Structure

```
Resources/
├── AppResources.xaml       # Colors, sizes, strings
├── Styles/
│   ├── Colors.xaml         # Color definitions
│   ├── Fonts.xaml          # Typography settings
│   └── Styles.xaml         # Control styles
├── Images/
│   ├── icon.png            # App icon
│   ├── splash.png          # Splash screen
│   └── [other images]
└── Fonts/
    ├── custom-font.ttf
    └── [other fonts]
```

## Best Practices

✅ Centralize theme colors and dimensions
✅ Use consistent naming conventions
✅ Define styles for common controls
✅ Store app strings in resource files (for localization)
✅ Organize images by category/size
✅ Use vector graphics (SVG) when possible

## Resource Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Colors | `[Usage]Color` | `PrimaryColor`, `TextColor`, `ErrorColor` |
| Fonts | `[Usage]Font` | `TitleFont`, `BodyFont` |
| Sizes | `[Usage]Size` | `TitleFontSize`, `IconSize` |
| Images | descriptive | `user_avatar.png`, `add_icon.svg` |

## Example AppResources.xaml

```xml
<ResourceDictionary>
    <!-- Colors -->
    <Color x:Key="PrimaryColor">#512BD4</Color>
    <Color x:Key="SecondaryColor">#DFD8F7</Color>
    <Color x:Key="TertiaryColor">#3B4E72</Color>
    <Color x:Key="TextColor">#212121</Color>
    <Color x:Key="LightTextColor">#666666</Color>
    <Color x:Key="ErrorColor">#F44336</Color>
    <Color x:Key="SuccessColor">#4CAF50</Color>

    <!-- Font Sizes -->
    <x:Double x:Key="FontSizeHeading">32</x:Double>
    <x:Double x:Key="FontSizeTitle">24</x:Double>
    <x:Double x:Key="FontSizeSubtitle">18</x:Double>
    <x:Double x:Key="FontSizeBody">14</x:Double>
    <x:Double x:Key="FontSizeSmall">12</x:Double>

    <!-- Spacing -->
    <x:Double x:Key="SpacingXSmall">4</x:Double>
    <x:Double x:Key="SpacingSmall">8</x:Double>
    <x:Double x:Key="SpacingMedium">16</x:Double>
    <x:Double x:Key="SpacingLarge">24</x:Double>

    <!-- Styles -->
    <Style TargetType="Label" x:Key="HeadingLabel">
        <Setter Property="FontSize" Value="{StaticResource FontSizeHeading}" />
        <Setter Property="TextColor" Value="{StaticResource TextColor}" />
        <Setter Property="FontAttributes" Value="Bold" />
    </Style>

    <Style TargetType="Label" x:Key="BodyLabel">
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}" />
        <Setter Property="TextColor" Value="{StaticResource TextColor}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>

    <Style TargetType="Button" x:Key="PrimaryButton">
        <Setter Property="Background" Value="{StaticResource PrimaryColor}" />
        <Setter Property="TextColor" Value="White" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}" />
        <Setter Property="Padding" Value="16,12" />
        <Setter Property="CornerRadius" Value="8" />
    </Style>
</ResourceDictionary>
```

## Using Resources in XAML

```xml
<Label 
    Text="Welcome"
    FontSize="{StaticResource FontSizeTitle}"
    TextColor="{StaticResource PrimaryColor}" />

<Button 
    Text="Click Me"
    Style="{StaticResource PrimaryButton}" />
```

## Using Resources in Code

```csharp
var primaryColor = Application.Current?.Resources["PrimaryColor"] as Color;
```
