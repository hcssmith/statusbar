# Statusbar

A modular, configurable Linux statusbar built with .NET 10 that displays system information via X11 root window.

## Features

- **Modular Block System** - Add custom blocks for any information you need
- **Hot-Reload Configuration** - Edit config files and see changes immediately
- **Color-Coded Output** - Dynamic colors based on system state (e.g., battery level)
- **Low Resource Usage** - Efficient polling with configurable intervals
- **Built-in Blocks**:
  - Time block with customizable format
  - Battery status with color-coded levels
  - Volume control via PulseAudio (`pactl`)
  - Command execution block for custom scripts
- **Multiple Config Support** - User config `~/.config/statusbar/appsettings.json` (required) with local `appsettings.json` override (optional)
- **Timestamped Logging** - Debug logs include timestamps for troubleshooting

## Requirements

- **.NET 10.0 Runtime** (or build as self-contained)
- **Linux x64**
- **X11** (sets root window name)
- **PulseAudio** (for volume block)
- **Optional**: `/sys/class/power_supply/` (for battery block)

## Installation

### Building from Source

```bash
# Build as self-contained single-file executable to ~/.local/bin
dotnet publish

# The executable will be at: ~/.local/bin/statusbar
```

### Configuration

Create required config file:

```bash
mkdir -p ~/.config/statusbar
# Create ~/.config/statusbar/appsettings.json (see Configuration section below)
```

## Configuration

Configuration is loaded from two sources (in order of priority, lowest to highest):

1. `~/.config/statusbar/appsettings.json` - **Required** - User-level configuration
2. `appsettings.json` (local directory) - **Optional** - Overrides user config for development
3. Environment variables
4. Command line arguments

### Example Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Services": "Debug"
    }
  },
  "Statusbar": {
    "PollInterval": 1000,
    "Blocks": [
      {
        "Type": "TimeBlock",
        "Order": 999,
        "Icon": "🕐 ",
        "Background": "#6A4C93",
        "Interval": 30000,
        "Format": "HH:mm"
      },
      {
        "Type": "BatteryBlock",
        "Order": 900,
        "BatteryLabel": "BAT0",
        "High": 70,
        "Low": 25,
        "HighIconDischarging": "🔋 ",
        "MediumIconDischarging": "🔋 ",
        "LowIconDischarging": "🪫 ",
        "HighIconCharging": "⚡ ",
        "MediumIconCharging": "⚡ ",
        "LowIconCharging": "⚡ ",
        "HighColour": "#3AA655",
        "MediumColour": "#C9A227",
        "LowColour": "#EE6C4D",
        "Interval": 30000
      },
      {
        "Type": "VolumeBlock",
        "Order": 800,
        "SpeakerIcon": "🔊 ",
        "HeadphonesIcon": "🎧 ",
        "Background": "#4DA8DA",
        "MuteIcon": "🔇 ",
        "MuteColour": "#8A8F98",
        "Interval": 5000
      },
      {
        "Type": "CommandBlock",
        "Id": "ip-address",
        "Timeout": 5000,
        "Order": 1,
        "Background": "#5FAF8E",
        "Icon": "🌐 ",
        "Command": "/home/user/.local/bin/ip.sh",
        "Interval": 30000
      }
    ]
  }
}
```

### Configuration Options

#### Statusbar Settings

- `PollInterval` (int): How often to update the bar, in milliseconds (default: `1000`)

#### Logging Settings

- `LogLevel.Default`: Minimum log level for all namespaces (e.g., `"Information"`, `"Debug"`)
- `LogLevel.Services`: Log level for Services namespace (e.g., `"Debug"`)

#### Block Settings

Each block in the `Blocks` array supports these common properties:

- `Type` (string): Block type - required
- `Id` (string): Unique identifier - required for multiple blocks of same type
- `Order` (int): Display position (lower = leftmost)
- `Icon` (string): Icon/text displayed before content
- `Background` (string): Background color in hex format (e.g., `"#2E2A44"`)
- `Foreground` (string): Text color in hex format (e.g., `"#ffffff"`)
- `Interval` (int): Update interval for this block, in milliseconds

Block-specific properties are documented below in the **Block System** section.

## Block System

### Built-in Block Types

#### TimeBlock

Displays current time with customizable format.

**Configuration:**
- `Format`: DateTime format string (e.g., `"HH:mm"`, `"HH:mm:ss"`)

#### BatteryBlock

Displays battery status with color-coded indicators based on charge level and charging state.

**Configuration:**
- `BatteryLabel`: Battery identifier (e.g., `"BAT0"`)
- `High`: Percentage considered "high" (e.g., `90`)
- `Low`: Percentage considered "low" (e.g., `20`)
- `HighIconDischarging`: Icon when high and discharging
- `MediumIconDischarging`: Icon when medium and discharging
- `LowIconDischarging`: Icon when low and discharging
- `HighIconCharging`: Icon when high and charging
- `MediumIconCharging`: Icon when medium and charging
- `LowIconCharging`: Icon when low and charging
- `HighColour`: Background color when high (e.g., `"#3AA655"`)
- `MediumColour`: Background color when medium (e.g., `"#C9A227"`)
- `LowColour`: Background color when low (e.g., `"#EE6C4D"`)

**Behavior:** Reads from `/sys/class/power_supply/{BatteryLabel}/` and updates icon/background based on capacity and charging state.

#### VolumeBlock

Displays volume level using `pactl` (PulseAudio).

**Configuration:**
- `SpeakerIcon`: Icon when using speakers
- `HeadphonesIcon`: Icon when using headphones
- `MuteIcon`: Icon when muted
- `MuteColour`: Background color when muted (e.g., `"#FFFFFF"`)
- `Timeout`: Command timeout in milliseconds (e.g., `1000`)

**Behavior:** Uses `pactl get-sink-volume`, `pactl get-sink-mute`, and `pactl list sinks` to determine volume, mute status, and output device.

#### CommandBlock

Executes a shell command and displays output.

**Configuration:**
- `Id`: Unique identifier (required for multiple CommandBlocks)
- `Command`: Full path to command/script (e.g., `"/home/user/.local/bin/ip.sh"`)
- `Timeout`: Command timeout in milliseconds (e.g., `5000`)

**Behavior:** Executes command at specified interval, captures stdout, and displays trimmed output. Returns empty string on error or timeout.

### Adding Custom Block Types

#### Step 1: Create Settings Class

Create `Blocks/{YourBlock}Settings.cs`:

```csharp
namespace Blocks;

public class YourBlockSettings : Settings {
  // Add block-specific properties
  public string YourProperty { get; set; } = "default";
  public int YourNumber { get; set; } = 42;
}
```

#### Step 2: Create Block Class

Create `Blocks/{YourBlock}.cs`:

```csharp
using Microsoft.Extensions.Logging;

namespace Blocks;

public class YourBlock : BlockBase {
  private YourBlockSettings _settings;

  public override void UpdateSettings<T>(T s) {
    _settings = (YourBlockSettings)(Settings)s;
    base.UpdateInternalSettings(s);
  }

  public YourBlock(ILogger<YourBlock> logger, YourBlockSettings settings) : base(logger, settings) {
    _settings = settings;
  }

  public override async Task<string> UpdateContent(CancellationToken ct) {
    // Implement your logic here
    await Task.Delay(100, ct);
    return "Your content";
  }
}
```

#### Step 3: Register Block Type

Modify `Services/StatusbarService.cs` in **two locations**:

**Location 1:** `OnSettingsChanged` method (around line 66)
**Location 2:** `ExecuteAsync` method (around line 122)

Add to both switch statements:

```csharp
case "YourBlock":
  CreateOrUpdate<YourBlock, YourBlockSettings>(block);
  break;
```

#### Step 4: Configure Block

Add to `appsettings.json`:

```json
{
  "Type": "YourBlock",
  "Order": 100,
  "Icon": "X ",
  "Background": "#123456",
  "Foreground": "#ffffff",
  "Interval": 5000,
  "YourProperty": "custom value",
  "YourNumber": 99
}
```

## Architecture

### Project Structure

```
statusbar/
├── Blocks/              # Block implementations
│   ├── BlockBase.cs     # Abstract base class for all blocks
│   ├── TimeBlock.cs     # Time display
│   ├── BatteryBlock.cs  # Battery status
│   ├── VolumeBlock.cs   # Volume control
│   └── CommandBlock.cs  # Command execution
├── Services/
│   └── StatusbarService.cs  # Main orchestrator service
├── X11/
│   ├── XRootWindow.cs   # X11 window management
│   └── xlib.cs        # X11 native bindings
├── Program.cs          # Application entry point
├── statusbar.csproj     # Project configuration
└── appsettings.json     # Example configuration
```

### Key Components

**Program.cs** - Configures .NET Host:
- Sets up configuration sources (user config, local config, environment variables)
- Configures logging with timestamps
- Registers `StatusbarService` as hosted service

**StatusbarService.cs** - Main orchestrator:
- Watches for configuration changes via `IOptionsMonitor`
- Creates/updates blocks when configuration changes
- Polls blocks and builds statusbar string
- Sets X11 root window name

**BlockBase.cs** - Abstract base for blocks:
- Inherits from `BackgroundService` (runs as background task)
- Calls `UpdateContent()` every `Interval` ms
- Formats output with colors and icons

**XRootWindow.cs** - X11 integration:
- Opens X11 display
- Sets `_NET_WM_NAME` and `WM_NAME` properties on root window
- Atom caching for efficiency

### Configuration Reloading

The application supports hot-reloading configuration changes:
- `reloadOnChange: true` on both config files
- `IOptionsMonitor<StatusbarSettings>` watches for changes
- `OnSettingsChanged` callback updates blocks when configuration changes
- Change detection prevents duplicate processing of same settings

## Development

### Building

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Publish self-contained single file to ~/.local/bin
dotnet publish

# Publish for specific platform
dotnet publish -r linux-x64 --self-contained true
```

### Project Settings

The project is configured for single-file self-contained publishing:

```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<RuntimeIdentifier>linux-x64</RuntimeIdentifier>
<PublishDir>$(HOME)/.local/bin</PublishDir>
```

### Dependencies

- **Microsoft.Extensions.Hosting** - Generic Host for background services
- **Microsoft.Extensions.Configuration** - Configuration system
- **Microsoft.Extensions.Logging** - Logging with timestamps
- **Microsoft.Extensions.Options** - Options monitoring with hot-reload

### Logging

Logs include timestamps in `[HH:mm:ss]` format.

**Enable debug logs:**
```json
{
  "Logging": {
    "LogLevel": {
      "Services": "Debug"
    }
  }
}
```

## Troubleshooting

### Statusbar not updating

- Check that `~/.config/statusbar/appsettings.json` exists
- Verify JSON syntax is valid
- Check logs for errors
- Ensure `PollInterval` is set appropriately

### Block showing "ERR"

- Check block-specific dependencies (e.g., PulseAudio for VolumeBlock)
- Verify file paths in CommandBlock are correct
- Check permissions for required system files
- Review logs for specific error messages

### Configuration changes not taking effect

- Ensure file is saved to correct location (`~/.config/statusbar/appsettings.json`)
- Check logs for configuration reload messages
- Verify JSON syntax is valid
- Local `appsettings.json` (in project directory) takes priority over user config

## Notes

readme auto generated by ai, no way I am writing all this.
