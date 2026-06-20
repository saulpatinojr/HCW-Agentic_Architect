# Behaviors

This folder contains **custom XAML behaviors** for encapsulating UI logic.

## Purpose

Behaviors attach reusable logic to UI elements without code-behind.

## Examples

- Event to command binding
- Numeric input validation
- Entry text formatting
- Animation triggers
- Focus behavior

## Structure

One behavior per file, name descriptively.

```csharp
using Microsoft.Maui.Controls;

public class NumericValidationBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry entry)
    {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
            return;

        if (!int.TryParse(e.NewTextValue, out _))
        {
            ((Entry)sender!).Text = e.OldTextValue ?? string.Empty;
        }
    }
}
```

## Usage in XAML

```xml
<Entry Placeholder="Enter number">
    <Entry.Behaviors>
        <behaviors:NumericValidationBehavior />
    </Entry.Behaviors>
</Entry>
```

## Common Behaviors

- `NumericValidationBehavior` - Allow only numbers
- `EmailValidationBehavior` - Email format validation
- `EventToCommandBehavior` - Route events to commands
- `FocusBehavior` - Auto-focus on load
- `MaxLengthBehavior` - Limit text input length
