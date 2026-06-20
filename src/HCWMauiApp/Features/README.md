# Features

This folder contains **feature modules** organized by business domain.

## Structure

Each feature follows this pattern:

```
[FeatureName]/
├── Pages/              # XAML pages for this feature
│   ├── [Page].xaml
│   └── [Page].xaml.cs
├── ViewModels/         # Feature-specific ViewModels
│   └── [Page]ViewModel.cs
├── Models/             # Feature-specific models (optional)
└── Services/           # Feature-specific services (optional)
```

## Creating a New Feature

1. Create folder under `Features/` with feature name (e.g., `Features/Users/`)
2. Create `Pages/`, `ViewModels/` subfolders
3. Create your XAML page and corresponding ViewModel
4. Inherit ViewModel from `BaseViewModel`
5. Register in `MauiProgram.cs`

## Example

```
Features/Users/
├── Pages/
│   ├── UsersPage.xaml
│   ├── UsersPage.xaml.cs
│   ├── UserDetailPage.xaml
│   └── UserDetailPage.xaml.cs
├── ViewModels/
│   ├── UsersPageViewModel.cs
│   └── UserDetailPageViewModel.cs
└── Models/
    └── UserFilter.cs
```

## Best Practices

✅ Keep features modular and self-contained
✅ Use feature-specific ViewModels
✅ Separate pages and logic
✅ Share common services from root `Services/`
✅ Add XML documentation to public members
