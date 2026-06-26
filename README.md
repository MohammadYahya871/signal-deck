# SignalDeck

SignalDeck is a Windows desktop app that plays audio automatically when specific events happen on your PC.

You create rules that decide:

- what audio to play
- which output device to use
- when playback should happen

Example:

> Play "welcome aboard captain, all systems online" on my main speakers when I come back to my PC after being away.

## Download

### Get SignalDeck

- [Download the latest release](https://github.com/MohammadYahya871/signal-deck/releases/latest)
- [Browse all releases](https://github.com/MohammadYahya871/signal-deck/releases)

If you just want to install the app, use the latest release link above.

## What SignalDeck Does

SignalDeck can play audio for events like:

- returning after idle
- Windows sign-in
- launching selected apps
- session lock
- session unlock
- resume from sleep

Each rule can define:

- a local audio file
- a specific output device
- a playback volume
- a trigger type
- trigger-specific settings
- cooldown behavior
- device fallback behavior

SignalDeck also pauses idle-time accumulation while media is actively playing, including common Windows media sessions exposed by browsers and media apps.

## Installation

1. Open the [latest release](https://github.com/MohammadYahya871/signal-deck/releases/latest).
2. Download the installer asset named like `SignalDeckSetup-x.y.z.exe`.
3. Run the installer.

The installer:

- installs SignalDeck into `C:\Program Files\SignalDeck`
- enables launch at sign-in by default
- supports in-place updates for future versions

## How To Use

1. Open SignalDeck.
2. Create a new rule or select an existing one.
3. Choose an audio file.
4. Choose the output device.
5. Set the volume.
6. Choose the trigger type.
7. Fill in any trigger-specific settings.
8. Choose what should happen if the selected device is unavailable.
9. Use `Preview Audio` to test the sound itself.
10. Use `Test Rule` to simulate the rule end-to-end.
11. Click `Save All`.

### Typical Example

To create a "welcome back" rule:

- choose your spoken greeting audio file
- choose your main speakers
- set the trigger to `Return After Idle`
- set the idle threshold to something like `5` minutes
- optionally set a cooldown
- save the rule

## Current Features

- multi-rule configuration
- per-rule output device selection
- per-rule volume control
- audio preview
- rule testing
- recent activity log
- device fallback options
- tray app behavior
- launch at sign-in support
- installer with upgrade support
- idle timing that ignores active media playback

## Building From Source

Requirements:

- Windows
- .NET 8 SDK

Build:

```powershell
dotnet build D:\work\tools\SignalDeck\SignalDeck.sln -c Release
```

## Packaging

SignalDeck includes:

- app icon: `assets/SignalDeck.ico`
- installer script: `installer/SignalDeck.iss`
- installer build script: `installer/build-installer.ps1`

Build the installer:

```powershell
powershell -ExecutionPolicy Bypass -File D:\work\tools\SignalDeck\installer\build-installer.ps1
```

Output artifacts:

- packaged app: `dist\publish\SignalDeck.exe`
- installer: `dist\installer\SignalDeckSetup-<version>.exe`

## GitHub Releases

SignalDeck is set up so installer versions are published through GitHub Releases instead of being committed into the repository.

Useful links:

- [Latest release](https://github.com/MohammadYahya871/signal-deck/releases/latest)
- [Release history](https://github.com/MohammadYahya871/signal-deck/releases)

Automation included in this repo:

- local release prep script: `scripts/new-release.ps1`
- GitHub Actions workflow: `.github/workflows/release-installer.yml`

### Release Flow

1. Prepare a new version locally:

```powershell
powershell -ExecutionPolicy Bypass -File D:\work\tools\SignalDeck\scripts\new-release.ps1 -Version 0.4.0
```

2. Push the branch and tag:

```powershell
git -C D:\work\tools\SignalDeck push Origin main
git -C D:\work\tools\SignalDeck push Origin v0.4.0
```

3. GitHub Actions builds the installer and publishes a GitHub Release with:

- `SignalDeckSetup-0.4.0.exe`

## Update Behavior

The installer is upgrade-aware:

- it reuses the same application identity
- it preserves the previous install directory
- it preserves previous task choices
- it updates an existing install in place
- it attempts to close `SignalDeck.exe` during upgrades

## Project Structure

- `src/SignalDeck.App`  
  WPF app, tray behavior, UI, view models
- `src/SignalDeck.Core`  
  shared models, interfaces, orchestration logic
- `src/SignalDeck.Infrastructure`  
  Windows integrations, playback, persistence, and triggers
- `installer/`  
  Inno Setup packaging
- `assets/`  
  icon and third-party notices
- `docs/`  
  product notes and design docs

## Notes

- SignalDeck is Windows-specific.
- Playback-aware idle detection depends on what Windows exposes through audio sessions and system media sessions.
- For mainstream browsers and media apps, this works much better than relying only on keyboard and mouse inactivity.

## License And Third-Party Assets

The current app icon is based on the Microsoft Fluent UI System Icons project.

See:

- [THIRD-PARTY-NOTICES.txt](D:\work\tools\SignalDeck\assets\THIRD-PARTY-NOTICES.txt)
- [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons)
