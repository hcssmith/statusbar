# Agent Instructions for Statusbar

## Build, Lint, Test Commands

### Build Commands
```bash
# Debug build (default)
dotnet build

# Release build
dotnet build -c Release

# Publish self-contained single-file to ~/.local/bin (configured in csproj)
dotnet publish

# Publish with explicit platform
dotnet publish -r linux-x64 --self-contained true

# Clean build artifacts
dotnet clean
```

### Test Commands
```bash
# Note: This project does not currently have unit tests
# When tests are added, use:
dotnet test                    # Run all tests
dotnet test --filter "FullyQualifiedName~ClassName"  # Run single test class
dotnet test --filter "FullyQualifiedName~MethodName"  # Run single test method
```

### Lint/Format
```bash
# This project uses .editorconfig for formatting (2-space indentation)
# Format on save is recommended via .editorconfig
# No explicit lint command configured
```

## Code Style Guidelines

### Imports and Namespaces
- **Implicit usings enabled** (`<ImplicitUsings>enable</ImplicitUsings>` in csproj)
- Explicit `using` statements only when needed outside implicit usings
- Group imports logically: System → Microsoft → Third-party → Local
- No unused using statements
- Alias imports: Use `using Alias = Type;` sparingly

### Formatting (from .editorconfig)
- **Indentation**: 2 spaces, no tabs
- **Brace style**: Allman for types (`class\n{`), K&R for methods/properties/control blocks
- **Spaces**: No spaces inside parentheses in method declarations/calls
- **Space after keywords**: `if (`, `while (`, `for (`
- **Binary operators**: Space before and after
- **Empty braces**: `{}` no space between (e.g., `new T() {}`)

```csharp
// Correct
public class MyClass {
  public void MyMethod(int x) {
    if (x > 0) {
      DoSomething(x);
    }
  }
}

// Incorrect
public class MyClass
{
  public void MyMethod( int x )
  {
    if( x > 0 )
    {
      DoSomething( x ) ;
    }
  }
}
```

### Naming Conventions
- **Classes**: `PascalCase` (e.g., `StatusbarService`, `TimeBlock`)
- **Methods/Properties**: `PascalCase` (e.g., `UpdateContent`, `PollInterval`)
- **Fields**: `_camelCase` with underscore prefix (e.g., `_settings`, `_logger`)
- **Local variables**: `camelCase` (e.g., `result`, `config`)
- **Constants**: `PascalCase` (e.g., `LibX11`, `MaxRetries`)
- **Interfaces**: `I` prefix (e.g., `IConfiguration`, `IOptionsMonitor`)
- **Generics**: `T` prefix for single type parameter (e.g., `TSettings`, `TBlock`)
- **Private methods**: `PascalCase` same as public methods
- **Async methods**: End with `Async` (e.g., `ExecuteAsync`, `UpdateContentAsync`)

### Types and Patterns
- **Prefer structs** for small immutable data (`Window`, `Atom`, `Display`)
- **Use abstract classes** when base functionality is needed (e.g., `BlockBase`)
- **Use interfaces** from libraries (e.g., `IConfiguration`, `IOptionsMonitor`)
- **Nullable reference types**: Enabled (`<Nullable>enable</Nullable>`)
- **Use async/await** for I/O operations, avoid `.Result` or `.GetAwaiter().GetResult()`

### Error Handling
- **Log errors** with `_logger.LogError()` including relevant context
- **Return error values** (e.g., `"ERR"`) rather than throwing for user-facing errors
- **Wrap exceptions** with context when re-throwing or logging
- **Use cancellation tokens**: Pass `CancellationToken ct` to async methods
- **Handle OperationCanceledException** explicitly for graceful shutdown

```csharp
// Good error handling
try {
  capacity = int.Parse(capacityText);
}
catch (Exception ex) {
  _logger.LogError($"Could not parse {capacityText} as a number: {ex.Message}");
  return Task.FromResult("ERR");
}
```

### Dependency Injection
- **Constructor injection**: All dependencies via constructor parameters
- **Use IServiceProvider** for creating instances with dependencies (e.g., `ActivatorUtilities.CreateInstance`)
- **Register services** in `Program.cs` using `ConfigureServices()`
- **Use IOptionsMonitor<T>** for configuration with hot-reload support

### Logging
- **Inject ILogger<T>**: Type-safe logging with category
- **Use appropriate log levels**:
  - `LogTrace()`: Very detailed diagnostic info
  - `LogDebug()`: Debug info during development
  - `LogInformation()`: Normal operational messages
  - `LogWarning()`: Recoverable issues
  - `LogError()`: Errors that don't stop the app
  - `LogCritical()`: Fatal errors requiring restart

### Configuration
- **IConfigurationSection** for nested configuration binding
- **Settings classes** inherit from base `Settings` class
- **Bind with Get<T>()**: `config.Get<TSettings>() ?? new TSettings() {}`
- **Required config**: `~/.config/statusbar/appsettings.json` (optional: false)
- **Local config**: `appsettings.json` for development overrides (optional: true)

### Block-Specific Guidelines
- **Inherit from BlockBase**: All blocks must inherit `BlockBase`
- **Implement abstract members**: `UpdateSettings<T>()` and `UpdateContent()`
- **Override virtual properties**: `Icon`, `Background`, `Foreground`, `Id` for dynamic behavior
- **Settings class pattern**: `{BlockName}Settings : Settings` with block-specific properties
- **UpdateInternalSettings()**: Always call after updating settings

### X11/Native Interop
- **Use static class** for native methods (e.g., `NativeXlib`)
- **DllImport** with `CallingConvention = CallingConvention.Cdecl`
- **MarshalAs** for string parameters: `[MarshalAs(UnmanagedType.LPStr)]`
- **Handle IntPtr carefully**: Check for `IntPtr.Zero` before use
- **Atom caching**: Cache atoms in dictionary to avoid repeated XInternAtom calls

## Project Structure
- **Blocks/**: All block implementations and settings classes
- **Services/**: Hosted services (main application logic)
- **X11/**: X11 interop and native bindings
- **Program.cs**: Application entry point and DI setup
- **statusbar.csproj**: Project configuration with publish settings

## Before Committing
- Build succeeds: `dotnet build`
- No compiler warnings (except necessary suppressions)
- Code follows .editorconfig formatting
- No unused using statements
- Documentation updated for new features
