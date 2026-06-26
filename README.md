# SignalDeck

SignalDeck is a Windows desktop app for event-driven audio playback.

It lets you create playback rules that answer three practical questions:

- what audio should play
- which output device should play it
- when it should play

SignalDeck was originally designed for personal "return to PC" moments like:

> play "welcome aboard captain, all systems online" on my main speakers when I come back to my computer

It has since been shaped into a broader rule-based automation tool for Windows audio events.

## What It Does

SignalDeck can create playback rules for events such as:

- returning after idle
- Windows sign-in
- launching selected apps
- session lock
- session unlock
- resume from sleep

Each rule can define:

- a local audio file
- a specific playback device
- a playback volume
- a trigger type
- trigger-specific settings
- a cooldown to avoid repeated playback

SignalDeck also pauses idle-time accumulation while media is actively playing, including common Windows media sessions exposed by browsers and media apps.

## Downloads

- [Latest release](https://github.com/MohammadYahya871/signal-deck/releases/latest)
- [All releases](https://github.com/MohammadYahya871/signal-deck/releases)

## Current Features

- multi-rule configuration
- per-rule output device selection
- per-rule volume control
- audio preview from the editor
- tray app behavior
- launch at sign-in support
- installer with upgrade support
- idle timing that ignores active media playback

## Installation

### Option 1: Install From The Packaged Installer

Run:

- [Download the latest installer from GitHub Releases](https://github.com/MohammadYahya871/signal-deck/releases/latest)

The installer:

- installs SignalDeck into `C:\Program Files\SignalDeck`
- registers it as a startup app by default
- supports in-place upgrades for future versions

### Option 2: Run The Packaged Build Directly

You can also run:

- [SignalDeck.exe](D:\work\tools\SignalDeck\dist\publish\SignalDeck.exe)

This is useful for quick local testing, but the installer is the recommended path for normal use.

## How To Use

1. Open SignalDeck.
2. Select a rule from the left side, or create one with `Add Rule`.
3. Set the `Audio file`.
4. Choose the `Output device`.
5. Adjust the `Volume`.
6. Choose the `Trigger type`.
7. Fill in any trigger-specific settings:
   - `Idle threshold` for `Return After Idle`
   - `Watched apps` for `When Selected App Starts`
8. Set a `Cooldown` if needed.
9. Use `Preview` to test the sound with the selected device and volume.
10. Click `Save All`.

### Typical Example

To create a "welcome back" rule:

- choose your spoken greeting audio file
- choose your main speakers
- set the trigger to `Return After Idle`
- set the idle threshold to something like `5` minutes
- optionally set a cooldown so it does not repeat too often

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

## Update Behavior

The installer is upgrade-aware:

- it reuses the same application identity
- it preserves the previous install directory
- it preserves previous task choices
- it can update an existing install in place
- it attempts to close `SignalDeck.exe` during upgrades

## GitHub Releases

The recommended way to publish installer versions is through GitHub Releases, not by committing `dist/` into the repository.

Useful links:

- [Latest release](https://github.com/MohammadYahya871/signal-deck/releases/latest)
- [Release history](https://github.com/MohammadYahya871/signal-deck/releases)

SignalDeck includes automation for that flow:

- local release prep script: `scripts/new-release.ps1`
- GitHub Actions workflow: `.github/workflows/release-installer.yml`

### How The Automated Release Flow Works

1. Update the app version and create a release commit + git tag locally.
2. Push `main` and the new version tag to GitHub.
3. GitHub Actions builds the installer.
4. GitHub Actions creates a GitHub Release for that tag.
5. Only the versioned installer is uploaded as the release asset.

### One-Time Setup

1. Create the GitHub repository.
2. Add your SSH public key to GitHub.
3. Push this local repository to the GitHub remote.
4. Make sure GitHub Actions are enabled for the repo.

### Create A New Release

Run:

```powershell
powershell -ExecutionPolicy Bypass -File D:\work\tools\SignalDeck\scripts\new-release.ps1 -Version 0.3.0
```

Then push:

```powershell
git -C D:\work\tools\SignalDeck push Origin main
git -C D:\work\tools\SignalDeck push Origin v0.3.0
```

After the tag is pushed, GitHub Actions will publish:

- `SignalDeckSetup-0.3.0.exe`

to the repo's Releases page automatically.

## Project Structure

- `src/SignalDeck.App`
  WPF app, tray behavior, UI, view models
- `src/SignalDeck.Core`
  shared models, interfaces, orchestration logic
- `src/SignalDeck.Infrastructure`
  Windows integrations, playback, persistence, triggers
- `installer/`
  Inno Setup packaging
- `assets/`
  icon and third-party notices
- `docs/`
  product notes and design docs

## Notes

- SignalDeck is intentionally Windows-specific.
- Some playback detection behavior depends on what Windows exposes through audio sessions and system media sessions.
- For mainstream browsers and media apps, this gives much better idle detection than relying on keyboard and mouse inactivity alone.

## License And Third-Party Assets

The current app icon is based on the Microsoft Fluent UI System Icons project.

See:

- [THIRD-PARTY-NOTICES.txt](D:\work\tools\SignalDeck\assets\THIRD-PARTY-NOTICES.txt)
- [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons)
