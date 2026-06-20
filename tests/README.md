# Tests

This folder contains **unit tests** and **integration tests** for the application.

## Structure

```
tests/
├── HCWMauiApp.Tests/           # Unit tests
│   ├── ViewModels/
│   ├── Services/
│   ├── Converters/
│   ├── Behaviors/
│   └── Utilities/
└── HCWMauiApp.Integration/     # Integration tests
    ├── Api/
    ├── Storage/
    └── Services/
```

## Test Project Setup

Create test projects with these references:

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.0.0" />
    <PackageReference Include="xunit" Version="2.4.1" />
    <PackageReference Include="Moq" Version="4.16.1" />
</ItemGroup>

<ItemGroup>
    <ProjectReference Include="..\..\src\HCWMauiApp\HCWMauiApp.csproj" />
</ItemGroup>
```

## Unit Test Example

```csharp
using Xunit;
using Moq;
using HCWMauiApp.Features.Home.ViewModels;
using HCWMauiApp.Services.Interfaces;

namespace HCWMauiApp.Tests.ViewModels;

public class HomePageViewModelTests
{
    [Fact]
    public async Task LoadDataAsync_WithValidData_SetsLoadingFalse()
    {
        // Arrange
        var mockService = new Mock<IExampleService>();
        mockService.Setup(s => s.GetDataAsync())
            .ReturnsAsync(new object());

        var viewModel = new HomePageViewModel(mockService.Object);

        // Act
        await viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadDataAsync_WithException_SetsErrorMessage()
    {
        // Arrange
        var mockService = new Mock<IExampleService>();
        mockService.Setup(s => s.GetDataAsync())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var viewModel = new HomePageViewModel(mockService.Object);

        // Act
        await viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(viewModel.ErrorMessage);
        Assert.Contains("Test error", viewModel.ErrorMessage);
    }
}
```

## Test Naming Convention

```
{MethodName}_{Scenario}_{ExpectedResult}
```

Examples:
- `LoadUsers_WithValidData_ReturnsUserList`
- `SaveUser_WithNullUser_ThrowsArgumentNullException`
- `GetUserById_WithInvalidId_ReturnsNull`

## Best Practices

✅ Test one thing per test
✅ Use Arrange-Act-Assert pattern
✅ Mock external dependencies
✅ Use descriptive test names
✅ Test both success and failure paths
✅ Aim for >80% code coverage
✅ Keep tests focused and fast
✅ Don't test MAUI framework code

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/HCWMauiApp.Tests/

# Run with coverage
dotnet test /p:CollectCoverageMetrics=true
```

## Mocking Best Practices

```csharp
var mockService = new Mock<IUserService>();

// Setup method
mockService.Setup(s => s.GetUserAsync(It.IsAny<int>()))
    .ReturnsAsync(new User { Id = 1, Name = "Test" });

// Setup with different returns based on argument
mockService.Setup(s => s.GetUserAsync(1))
    .ReturnsAsync(new User { Id = 1, Name = "User1" });
mockService.Setup(s => s.GetUserAsync(2))
    .ReturnsAsync(new User { Id = 2, Name = "User2" });

// Setup to throw exception
mockService.Setup(s => s.GetUserAsync(-1))
    .ThrowsAsync(new ArgumentException("Invalid user ID"));

// Verify the method was called
mockService.Verify(s => s.GetUserAsync(It.IsAny<int>()), Times.Once);
```

See [xUnit docs](https://xunit.net/) and [Moq docs](https://github.com/moq/moq4)
