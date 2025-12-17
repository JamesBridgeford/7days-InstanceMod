# Development Guide

## Prerequisites

- **7 Days to Die** Alpha 20+ installed
- **Visual Studio 2022** or **JetBrains Rider**
- **.NET Framework 4.8 SDK** or higher
- **Git** for version control

## Project Setup

### 1. Clone the Repository

```bash
git clone https://github.com/JamesBridgeford/7days-InstanceMod.git
cd 7days-InstanceMod
```

### 2. Configure Game Path

Edit `InstanceMod.csproj` and set your game installation path:

```xml
<PropertyGroup>
    <GamePath>C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die</GamePath>
</PropertyGroup>
```

### 3. Restore References

The project references game DLLs from `$(GamePath)\7DaysToDie_Data\Managed\`. Ensure this path is correct.

Required DLLs:
- `Assembly-CSharp.dll`
- `Assembly-CSharp-firstpass.dll`
- `LogLibrary.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `0Harmony.dll`

### 4. Build the Project

**Debug Configuration:**
- Outputs to `InstanceMod/` folder
- Includes debug symbols
- Auto-copies Unity dependencies

**Release Configuration:**
- Optimized build
- Minimal debug info
- Production-ready

```bash
# Command line build
msbuild InstanceMod.sln /p:Configuration=Release

# Or build in IDE
```

## Project Structure

```
7days-InstanceMod/
├── src/InstanceMod/           # Source code
│   ├── API.cs                 # Main ModAPI entry point
│   ├── Core/                  # Core systems
│   │   ├── Instance.cs        # Instance data model
│   │   ├── InstanceManager.cs # Instance management
│   │   └── PlayerInstanceData.cs # Player data
│   ├── Harmony/               # Harmony patches (future)
│   └── Commands/              # Console commands
│       └── InstanceCommand.cs # Instance command handler
├── InstanceMod/               # Build output (mod folder)
│   ├── Config/                # XML configurations
│   ├── ModInfo.xml           # Mod metadata
│   └── InstanceMod.dll       # Compiled mod
├── Properties/
│   └── AssemblyInfo.cs       # Assembly metadata
└── docs/                      # Documentation
```

## Development Workflow

### 1. Make Changes

Edit source files in `src/InstanceMod/`

### 2. Build

Build the project in your IDE or via command line.

### 3. Test

The compiled DLL is output to `InstanceMod/`, which can be copied directly to your game's Mods folder.

**Manual Installation:**
```bash
# Copy InstanceMod folder to game's Mods directory
cp -r InstanceMod/ "C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/"
```

**Automatic Installation (Optional):**

Add post-build event to `.csproj`:
```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="xcopy &quot;$(OutputPath)*&quot; &quot;$(GamePath)\Mods\InstanceMod\&quot; /Y /E /I" />
</Target>
```

### 4. Launch Game

Start 7 Days to Die and check the console (F1) for initialization messages:
```
[InstanceMod] Version 0.1.0 - Initializing...
[InstanceMod] Harmony patches applied successfully
[InstanceMod] Event handlers registered
[InstanceMod] Successfully initialized!
```

### 5. Debug

Check logs in:
- **In-game console**: Press F1
- **Log file**: `7 Days To Die/output_log.txt`
- **Player log**: `AppData/LocalLow/The Fun Pimps/7 Days To Die/Player.log`

## Testing

### Manual Testing

1. **Launch Dedicated Server** or local game
2. **Open Console** (F1)
3. **Test Commands:**
   ```
   instance list
   instance create TestInstance "My test instance"
   instance join TestInstance
   instance info
   instance leave
   ```

### Test Checklist

- [ ] Mod loads without errors
- [ ] Default instance is created
- [ ] Create new instances
- [ ] Join/leave instances
- [ ] List instances shows correct data
- [ ] Player data is tracked per instance
- [ ] Instance deletion works (admin only)
- [ ] Multiple players can join same instance
- [ ] Player data persists across sessions (once implemented)

## Code Style Guidelines

### Naming Conventions

- **Classes**: PascalCase - `InstanceManager`
- **Methods**: PascalCase - `CreateInstance()`
- **Fields**: camelCase with prefix - `_privateField`, `publicField`
- **Constants**: UPPER_CASE - `DEFAULT_INSTANCE_ID`
- **Properties**: PascalCase - `PlayerData`

### Documentation

- Use XML comments for public APIs
- Include `<summary>`, `<param>`, and `<returns>` tags
- Explain WHY, not just WHAT

Example:
```csharp
/// <summary>
/// Creates a new isolated game instance.
/// Instances allow separate progression and resources for different player groups.
/// </summary>
/// <param name="name">Unique display name for the instance</param>
/// <param name="ownerId">Player ID of the instance owner</param>
/// <returns>Created Instance object, or null if creation failed</returns>
public static Instance CreateInstance(string name, string ownerId)
{
    // Implementation
}
```

### Error Handling

Always use try-catch in:
- Event handlers
- Command execution
- Public APIs

```csharp
try
{
    // Risky operation
}
catch (Exception ex)
{
    Log.Error($"[InstanceMod] Operation failed: {ex.Message}");
    Log.Error($"[InstanceMod] Stack trace: {ex.StackTrace}");
}
```

### Logging

Use consistent log prefixes:
```csharp
Log.Out($"[InstanceMod] Info message");
Log.Warning($"[InstanceMod] Warning message");
Log.Error($"[InstanceMod] Error message");
```

## Adding New Features

### 1. Plan the Feature

Document in GitHub Issues:
- What it does
- Why it's needed
- How it will work
- Potential impacts

### 2. Create Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 3. Implement

Follow existing code patterns:
- Core logic in `Core/`
- Harmony patches in `Harmony/`
- Commands in `Commands/`

### 4. Test Thoroughly

- Unit test logic where possible
- Manual testing in-game
- Test with multiple players
- Test edge cases

### 5. Document

- Update README.md if needed
- Add XML comments
- Update CHANGELOG.md

### 6. Submit Pull Request

Include:
- Description of changes
- Testing performed
- Any breaking changes

## Common Issues

### Build Errors

**Missing References:**
- Verify `GamePath` in `.csproj`
- Check that game DLLs exist in `Managed/` folder

**Namespace Errors:**
- Ensure all `using` statements are correct
- Rebuild solution

### Runtime Errors

**Mod Not Loading:**
- Check `ModInfo.xml` is valid
- Verify DLL is in correct location
- Check output_log.txt for errors

**Harmony Patches Failing:**
- Verify target class/method names
- Check game version compatibility
- Ensure method signatures match

**NullReferenceException:**
- Add null checks for ClientInfo
- Validate instance IDs before use
- Check dictionary keys exist before access

## Useful Tools

### Debugging

**dnSpy** - .NET debugger and decompiler
- View game DLL internals
- Debug mod code
- Useful for reverse engineering

**ILSpy** - IL decompiler
- Browse game assemblies
- Find method signatures

### Development

**Visual Studio 2022** - Full-featured IDE
- IntelliSense
- Debugging
- Git integration

**JetBrains Rider** - Alternative IDE
- Better performance
- Cross-platform
- Advanced refactoring

## Resources

- **7DTD Modding Wiki**: https://7daystodie.fandom.com/wiki/ModAPI
- **Harmony Docs**: https://harmony.pardeike.net/
- **ServerTools Example**: https://github.com/dmustanger/7dtd-ServerTools
- **Official Forums**: https://community.thefunpimps.com/

## Getting Help

- **GitHub Issues**: Report bugs or request features
- **GitHub Discussions**: Ask questions
- **Discord**: Coming soon

---

Happy modding! 🎮
