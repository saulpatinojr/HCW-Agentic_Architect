# Models

This folder contains **domain models** and **data transfer objects (DTOs)**.

## Structure

```
Models/
├── Domain/             # Core business entities
│   ├── User.cs
│   └── Product.cs
├── DTOs/               # API response/request objects
│   ├── UserDTO.cs
│   └── ProductDTO.cs
└── Responses/          # API response wrappers
    └── ApiResponse.cs
```

## Best Practices

✅ Keep models as simple data structures
✅ Use properties, minimal logic
✅ Use nullable reference types
✅ Implement `INotifyPropertyChanged` in ViewModels, NOT in models
✅ Use **records** for immutable DTOs
✅ Add XML documentation

## Examples

**Domain Model:**
```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

**DTO (for APIs):**
```csharp
public record UserDTO(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt
);
```

**Response Wrapper:**
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}
```
