# HCW Workspace Manager - Complete Documentation Index

## Quick Navigation

### 🚀 **Getting Started** (Start Here!)
1. **[MAUI-QUICK-REFERENCE.md](MAUI-QUICK-REFERENCE.md)** ← Start here for hands-on examples
   - Feature creation step-by-step
   - Service creation patterns
   - Common code snippets
   - XAML binding examples
   - Testing patterns

2. **[src/HCWMauiApp/README.md](src/HCWMauiApp/README.md)** - Architecture overview
   - Project structure
   - Quick start guide
   - Key files
   - Naming conventions

### 📚 **Detailed Architecture**
- **[docs/application/MAUI-ARCHITECTURE.md](docs/application/MAUI-ARCHITECTURE.md)** - Complete architecture guide
  - MVVM pattern explained
  - Feature organization
  - Service patterns with examples
  - Error handling strategies
  - Testing strategy
  - Performance tips
  - Common pitfalls

- **[src/README.md](src/README.md)** - Full directory structure guide
  - What belongs where
  - File organization
  - Architecture principles
  - Getting started process
  - Best practices checklist

### 📖 **Directory-Specific Guides**

#### Application Structure
| Directory | Purpose | README |
|-----------|---------|--------|
| **Features/** | Feature modules by domain | [Features/README.md](src/HCWMauiApp/Features/README.md) |
| **Services/** | Business logic & APIs | [Services/README.md](src/HCWMauiApp/Services/README.md) |
| **ViewModels/** | MVVM presentation layer | [ViewModels/README.md](src/HCWMauiApp/ViewModels/README.md) |
| **Models/** | Domain entities & DTOs | [Models/README.md](src/HCWMauiApp/Models/README.md) |
| **Resources/** | Images, fonts, styles | [Resources/README.md](src/HCWMauiApp/Resources/README.md) |
| **Converters/** | XAML value converters | [Converters/README.md](src/HCWMauiApp/Converters/README.md) |
| **Behaviors/** | Reusable UI behaviors | [Behaviors/README.md](src/HCWMauiApp/Behaviors/README.md) |
| **Utilities/** | Helpers & extensions | [Utilities/README.md](src/HCWMauiApp/Utilities/README.md) |
| **Platforms/** | Platform-specific code | [Platforms/README.md](src/HCWMauiApp/Platforms/README.md) |
| **Tests/** | Unit & integration tests | [tests/README.md](tests/README.md) |

### 📋 **Summary Documents**
- **[src/HCWMauiApp/SETUP-SUMMARY.md](src/HCWMauiApp/SETUP-SUMMARY.md)** - What's been created and next steps
  - Complete file structure
  - Templates provided
  - High priority next steps
  - Implementation roadmap

---

## Complete Directory Tree

```
HCW-WorkspaceManager/
│
├── 📄 MAUI-QUICK-REFERENCE.md          ← START HERE (examples & patterns)
├── 📄 MAUI-COMPLETE-INDEX.md            ← This file (navigation)
│
├── docs/
│   ├── application/
│   │   └── 📘 MAUI-ARCHITECTURE.md     (detailed architecture guide)
│   └── [other docs...]
│
├── src/
│   ├── 📘 README.md                    (directory structure explanation)
│   │
│   └── HCWMauiApp/                     ← Main Application
│       ├── 📘 README.md                (quick start)
│       ├── 📄 SETUP-SUMMARY.md         (implementation roadmap)
│       │
│       ├── Features/                   ← Feature modules
│       │   ├── 📘 README.md            (feature creation guide)
│       │   ├── Home/
│       │   │   ├── Pages/
│       │   │   │   ├── HomePage.xaml   (template page)
│       │   │   │   └── HomePage.xaml.cs
│       │   │   └── ViewModels/
│       │   │       └── HomePageViewModel.cs (template ViewModel)
│       │   └── Settings/
│       │
│       ├── ViewModels/                 ← Presentation Layer
│       │   ├── 📘 README.md            (ViewModel patterns)
│       │   ├── Base/
│       │   │   └── BaseViewModel.cs    (template base class)
│       │   └── AppShellViewModel.cs
│       │
│       ├── Services/                   ← Business Logic
│       │   ├── 📘 README.md            (service patterns)
│       │   ├── Interfaces/
│       │   │   └── IExampleService.cs  (template interface)
│       │   ├── Http/
│       │   ├── Storage/
│       │   ├── Auth/
│       │   ├── Navigation/
│       │   └── ExampleService.cs       (template implementation)
│       │
│       ├── Models/                     ← Data Layer
│       │   ├── 📘 README.md            (model patterns)
│       │   ├── Domain/
│       │   ├── DTOs/
│       │   ├── Responses/
│       │   └── ExampleModel.cs         (template model)
│       │
│       ├── Resources/                  ← App Resources
│       │   ├── 📘 README.md            (styling guide)
│       │   ├── Styles/
│       │   ├── Images/
│       │   └── Fonts/
│       │
│       ├── Converters/                 ← XAML Converters
│       │   ├── 📘 README.md            (converter guide)
│       │   └── BoolToColorConverter.cs (template converter)
│       │
│       ├── Behaviors/                  ← XAML Behaviors
│       │   └── 📘 README.md            (behavior guide)
│       │
│       ├── Utilities/                  ← Helpers & Extensions
│       │   ├── 📘 README.md            (utility guide)
│       │   ├── Constants/
│       │   ├── Extensions/
│       │   │   └── CollectionExtensions.cs
│       │   └── Helpers/
│       │
│       └── Platforms/                  ← Platform-Specific
│           ├── 📘 README.md            (platform guide)
│           ├── iOS/
│           ├── Android/
│           ├── Windows/
│           └── MacCatalyst/
│
└── tests/                              ← Test Suite
    ├── 📘 README.md                    (test structure & patterns)
    ├── HCWMauiApp.Tests/               (unit tests)
    └── HCWMauiApp.Integration/         (integration tests)
```

---

## 🎯 Common Tasks & Where to Find Guidance

### **I want to...**

| Task | See This |
|------|----------|
| **Create a new feature** | [MAUI-QUICK-REFERENCE.md](MAUI-QUICK-REFERENCE.md#starting-a-new-feature) + [Features/README.md](src/HCWMauiApp/Features/README.md) |
| **Create a service** | [MAUI-QUICK-REFERENCE.md](MAUI-QUICK-REFERENCE.md#creating-a-service) + [Services/README.md](src/HCWMauiApp/Services/README.md) |
| **Understand MVVM** | [docs/application/MAUI-ARCHITECTURE.md](docs/application/MAUI-ARCHITECTURE.md#mvvm-pattern) |
| **Add a new service to DI** | [MAUI-QUICK-REFERENCE.md](MAUI-QUICK-REFERENCE.md#step-3-register-in-mauiprogramcs) |
| **Learn dependency injection** | [Services/README.md](src/HCWMauiApp/Services/README.md#service-lifetimes) |
| **Build a XAML page** | [MAUI-QUICK-REFERENCE.md](MAUI-QUICK-REFERENCE.md#xaml-binding-patterns) |
| **Use data binding** | [MAUI-QUICK-REFERENCE.md](MAUI-QUICK-REFERENCE.md#xaml-binding-patterns) |
| **Create a converter** | [Converters/README.md](src/HCWMauiApp/Converters/README.md) |
| **Create a behavior** | [Behaviors/README.md](src/HCWMauiApp/Behaviors/README.md) |
| **Handle errors** | [docs/application/MAUI-ARCHITECTURE.md](docs/application/MAUI-ARCHITECTURE.md#error-handling) |
| **Write unit tests** | [tests/README.md](tests/README.md) + [MAUI-QUICK-REFERENCE.md](MAUI-QUICK-REFERENCE.md#testing-patterns) |
| **Use extension methods** | [Utilities/README.md](src/HCWMauiApp/Utilities/README.md#extension-methods-example) |
| **Organize resources** | [Resources/README.md](src/HCWMauiApp/Resources/README.md) |
| **Work with models** | [Models/README.md](src/HCWMauiApp/Models/README.md) |
| **Handle platform-specific code** | [Platforms/README.md](src/HCWMauiApp/Platforms/README.md) |

---

## 📋 Template Files Provided

All of these are example implementations showing best practices:

1. **BaseViewModel.cs** - MVVM base class with property notifications
2. **HomePageViewModel.cs** - Example feature ViewModel
3. **HomePage.xaml** - Sample XAML page
4. **HomePage.xaml.cs** - Code-behind example
5. **IExampleService.cs** - Service interface template
6. **ExampleService.cs** - Service implementation template
7. **BoolToColorConverter.cs** - XAML converter example
8. **ExampleModel.cs** - Data model example
9. **CollectionExtensions.cs** - Extension method example

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│            XAML Pages (View Layer)                  │
│                                                      │
│   Binding Context → ViewModel                       │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│      ViewModels (Presentation Layer)                │
│                                                      │
│  • Inherit from BaseViewModel                       │
│  • Handle state & commands                          │
│  • Implement INotifyPropertyChanged                 │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼ (Calls)
┌─────────────────────────────────────────────────────┐
│      Services (Business Logic Layer)                │
│                                                      │
│  • Have interfaces (IMyService)                     │
│  • Registered in DI container                       │
│  • Handle API calls, data access                    │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼ (Works with)
┌─────────────────────────────────────────────────────┐
│      Models (Data Layer)                            │
│                                                      │
│  • Simple data structures                           │
│  • Properties, minimal logic                        │
│  • Domain entities & DTOs                           │
└─────────────────────────────────────────────────────┘
```

---

## ✅ Implementation Checklist

### Phase 1: Foundation
- [ ] Create project file (`HCWMauiApp.csproj`)
- [ ] Create `App.xaml` with `AppResources.xaml`
- [ ] Create `AppShell.xaml` with navigation routes
- [ ] Create `MauiProgram.cs` with DI configuration
- [ ] Set up logging

### Phase 2: Core Services
- [ ] Create `IApiClient` and `ApiClient`
- [ ] Create `IAuthService` and `AuthService`
- [ ] Create `IStorageService` and `StorageService`
- [ ] Set up HTTP client configuration

### Phase 3: First Feature
- [ ] Build Home feature completely
- [ ] Create HomePage and HomePageViewModel
- [ ] Add sample service integration
- [ ] Test navigation

### Phase 4: Quality
- [ ] Create unit tests for ViewModels
- [ ] Create unit tests for Services
- [ ] Set up CI/CD
- [ ] Document custom services

---

## 🔗 Related Resources

### Official Documentation
- [MAUI Docs](https://learn.microsoft.com/dotnet/maui)
- [MVVM Toolkit](https://learn.microsoft.com/windows/communitytoolkit/mvvm/mvvm_introduction)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

### Dependencies (Recommended)
- `CommunityToolkit.Mvvm` - MVVM helpers
- `Serilog` - Structured logging
- `Refit` - REST client
- `Xunit` - Unit testing
- `Moq` - Mocking library

---

## 📞 Document Legend

- 📘 = Comprehensive guide (read for deep understanding)
- 📄 = Quick reference (use for examples)
- ✅ = Checklist
- 🔗 = External reference

---

**Last Updated**: 2024 - Complete MAUI application skeleton with best practices documentation
