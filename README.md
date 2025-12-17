# 7 Days to Die - Instance Mod

![GitHub release](https://img.shields.io/github/v/release/JamesBridgeford/7days-InstanceMod)
![License](https://img.shields.io/github/license/JamesBridgeford/7days-InstanceMod)

## Overview

The Instance Mod creates isolated game instances within 7 Days to Die, allowing for dedicated resources, progression tracking, and unique gameplay experiences within the same server environment.

## Features

- **Isolated Instances**: Create separate game instances with dedicated resources
- **Progression Tracking**: Track player progress independently per instance
- **Resource Management**: Isolated loot, spawns, and world state per instance
- **Admin Controls**: Comprehensive admin commands for instance management
- **Multiplayer Support**: Full support for dedicated servers and multiplayer sessions

## Requirements

- **7 Days to Die**: Alpha 20+ (tested on Alpha 21)
- **.NET Framework**: 4.8 or 4.8.1
- **Server Type**: Dedicated server or local game
- **Dependencies**: Harmony patching library (included)

## Installation

### Server Installation

1. Download the latest release from [Releases](https://github.com/JamesBridgeford/7days-InstanceMod/releases)
2. Extract the `InstanceMod` folder to your `7DaysToDie/Mods/` directory
3. Restart your server
4. Check the console logs for successful initialization: `[InstanceMod] Initialized successfully`

### Client Installation

XML configurations will sync automatically from the server. No client-side installation required for basic functionality.

## Usage

### Basic Commands

```
instance create <name> - Create a new instance
instance list - List all available instances
instance join <name> - Join a specific instance
instance leave - Leave current instance
instance delete <name> - Delete an instance (admin only)
instance info [name] - Get instance information
```

### Admin Commands

```
instance admin add <playerId> <instanceName> - Grant admin access
instance admin remove <playerId> <instanceName> - Revoke admin access
instance config <instanceName> <setting> <value> - Configure instance settings
instance reset <instanceName> - Reset instance data (admin only)
```

## Configuration

Configuration files are located in `InstanceMod/Config/`:

- `instances.xml` - Instance definitions and settings
- `permissions.xml` - Permission management
- `blocks.xml` - Block modifications (if any)
- `items.xml` - Item modifications (if any)

## Development

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/JamesBridgeford/7days-InstanceMod.git
   cd 7days-InstanceMod
   ```

2. Open the solution in Visual Studio 2022 or JetBrains Rider

3. Set your game installation path in the `.csproj` file:
   ```xml
   <GamePath>C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die</GamePath>
   ```

4. Build the project (Release configuration)

5. The compiled mod will be output to `InstanceMod/` folder

### Project Structure

```
7days-InstanceMod/
├── InstanceMod/              # Compiled mod output
│   ├── Config/               # XML configurations
│   ├── Resources/            # Unity assets
│   ├── ModInfo.xml          # Mod metadata
│   └── InstanceMod.dll      # Compiled C# code
├── src/                      # Source code
│   └── InstanceMod/
│       ├── Core/            # Core functionality
│       ├── Harmony/         # Harmony patches
│       └── Commands/        # Console commands
├── docs/                     # Documentation
└── .github/                  # GitHub workflows
```

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -am 'Add new feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Submit a pull request

## Roadmap

- [x] Project initialization and structure
- [ ] Core instance management system
- [ ] Player data isolation
- [ ] World state management
- [ ] Admin command implementation
- [ ] Configuration system
- [ ] Testing and optimization
- [ ] Documentation completion
- [ ] Initial release (v1.0.0)

## Known Issues

None yet - this is a new project!

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/JamesBridgeford/7days-InstanceMod/issues)
- **Discussions**: [GitHub Discussions](https://github.com/JamesBridgeford/7days-InstanceMod/discussions)
- **Discord**: Coming soon

## Credits

- **Author**: JamesBridgeford
- **Harmony**: [Pardeike's Harmony](https://github.com/pardeike/Harmony)
- **Inspiration**: ServerTools mod by dmustanger

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

**Note**: This mod is in active development. Features and APIs may change between versions.
