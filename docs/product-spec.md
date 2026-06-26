# SignalDeck Product Spec

## Product Vision

SignalDeck is a Windows desktop utility for event-driven audio playback.

Users create named rules that bind:

- an audio file
- an output device
- playback behavior
- a trigger condition

SignalDeck is intended for personal automation moments such as:

- "Play my return greeting when I come back to my PC"
- "Play a sound when a specific app launches"
- "Play a sound when Windows signs me in"

The design goal is to make these moments feel intentional and delightful while keeping the product understandable.

## Core Product Model

SignalDeck should be modeled as a rule-based automation app, not just an audio player.

### Core Entities

#### Rule

A user-defined automation item.

Suggested fields:

- `id`
- `name`
- `enabled`
- `trigger`
- `playback`
- `cooldown`
- `createdAt`
- `updatedAt`

#### Trigger

Defines when a rule should fire.

Phase 1 will support one trigger type only:

- `return_after_idle`

Future trigger types:

- `sign_in`
- `app_launch`
- `session_lock`
- `session_unlock`
- `resume_from_sleep`

#### Playback Settings

Defines how audio should be played.

Suggested fields:

- `audioFilePath`
- `outputDeviceId`
- `outputDeviceNameSnapshot`
- `volumeMode`
- `volumeValue`

#### Cooldown Policy

Prevents overly frequent replay.

Suggested fields:

- `enabled`
- `seconds`

## Phase Plan

## Phase 1

### Goal

Ship a stable first version that fully supports this use case:

- play a specific audio clip
- on a chosen audio output device
- at a chosen volume
- when the user returns after being away from the PC

### Scope

Phase 1 includes:

- a single-rule experience or a rule list with support for one real trigger type
- choosing one local audio file
- choosing one Windows output device
- choosing one playback volume
- choosing an idle threshold
- choosing a cooldown
- detecting user return after idle
- background operation while the user session is active
- basic persistence of settings

### Trigger Definition For Phase 1

Trigger type:

- `return_after_idle`

Trigger semantics:

- the user has been idle for at least a configured threshold
- the rule fires when user activity resumes

Suggested trigger fields:

- `type: "return_after_idle"`
- `idleThresholdSeconds`
- `requireSessionActive: true`

Notes:

- This is closer to the real human intention than lock/unlock alone.
- It should not fire for trivial pauses shorter than the threshold.

### Playback Behavior For Phase 1

Required behavior:

- play one chosen audio file
- route playback to one chosen output device if available
- apply one chosen playback volume for that rule

Open product choice for implementation:

- either apply per-playback volume inside the app audio pipeline
- or temporarily adjust device/session volume only if absolutely necessary

Preferred direction:

- keep playback volume scoped to SignalDeck itself, not global system volume

### Cooldown Behavior

Phase 1 should include cooldown support.

Reason:

- without cooldown, brief repeated idle/return cycles may become annoying

Suggested behavior:

- a rule can only fire if its cooldown has expired
- cooldown is measured from the last successful playback start

### Device Unavailable Behavior

If the chosen device is missing:

- preferred Phase 1 default: skip playback and record the failure state

Why:

- silent fallback to another device may surprise the user

Possible future option:

- allow optional fallback to the default output device

### Recommended UX Shape For Phase 1

The UI should be simple and direct.

Suggested fields:

- `Rule name`
- `Audio file`
- `Output device`
- `Volume`
- `Trigger: Return after idle`
- `Idle threshold`
- `Cooldown`
- `Enabled`

Suggested first-run framing:

- "Play a sound when I return to my PC"

That wording is more intuitive than exposing low-level event language first.

### Non-Goals For Phase 1

Do not include these yet:

- multiple trigger types in the UI
- advanced rule conflict resolution
- machine-wide service behavior before user sign-in
- cloud sync
- playlists
- looping audio
- app launch monitoring
- lock/unlock automation
- wake-from-sleep automation

## Phase 2

### Goal

Expand SignalDeck into a general Windows audio event automation tool while preserving the Phase 1 mental model.

### Planned Additions

- multiple rules as a first-class experience
- more trigger types
- app launch matching for selected executables
- sign-in trigger
- lock trigger
- unlock trigger
- resume-from-sleep trigger
- richer device fallback behavior
- better error visibility and lightweight logs
- overlap handling policies
- import/export of settings

### Planned Trigger Catalog

#### `sign_in`

For when the user begins a Windows session.

#### `app_launch`

For when any selected executable starts.

Suggested options:

- exact executable match list
- fire once per launch
- ignore already-running apps at SignalDeck startup

#### `session_lock`

For when the session becomes locked.

#### `session_unlock`

For when the session is unlocked.

#### `resume_from_sleep`

For when Windows resumes from sleep.

Possible future refinement:

- separate "system resumed" from "user became active after resume"

## Architecture Direction

Phase 1 should be implemented using abstractions that can survive Phase 2.

Suggested internal modules:

- `RuleStore`
- `TriggerEngine`
- `TriggerMonitor`
- `PlaybackEngine`
- `AudioDeviceResolver`
- `CooldownEvaluator`

### Design Principle

Do not hardcode the Subnautica example into the product.

Instead:

- define a generic rule model
- implement one concrete trigger type well
- preserve extension points for future trigger monitors

## Key Product Decisions

### Rule Granularity

Each rule should have one trigger type.

Why:

- easier to understand
- easier to test
- easier to extend later

If a user wants the same sound for multiple events, they create multiple rules.

### Output Device Selection

Selection should be explicit and stored using a stable device identifier when possible, plus a friendly name snapshot for display.

### Volume Control

Per-rule volume is a first-class feature, not a future enhancement.

This is important for cinematic or spoken greeting clips where the user wants predictable loudness.

## Example Rule

This is the motivating Phase 1 example:

- `Name`: Return greeting
- `Audio file`: "welcome aboard captain, all systems online"
- `Output device`: Main speakers
- `Volume`: 45%
- `Trigger`: Return after idle
- `Idle threshold`: 5 minutes
- `Cooldown`: 10 minutes

## Success Criteria For Phase 1

Phase 1 is successful if:

- the user can configure the return greeting without editing files manually
- the configured sound reliably plays when the user returns after the chosen idle threshold
- the sound plays on the selected output device
- the configured volume is honored
- the app avoids obvious repeated-fire annoyance through cooldowns
- the internal design does not block additional trigger types later
