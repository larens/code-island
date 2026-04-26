<h1 align="center">
  <img src="docs/images/code-island-icon.svg" width="64" height="64" alt="Code Island app icon" valign="middle">&nbsp;
  Code Island
</h1>
<p align="center">
  <b>Dynamic Island-style AI coding session monitor for the macOS menu bar</b><br>
  <a href="https://erha19.github.io/">Website</a> •
  <a href="#installation">Install</a> •
  <a href="#features">Features</a> •
  <a href="#buddy-detach">Buddy Detach</a> •
  <a href="#supported-tools">Supported Tools</a> •
  <a href="#build-from-source">Build</a><br>
  English | <a href="README.zh-CN.md">简体中文</a>
</p>

<p align="center">
  <a href="https://github.com/erha19/code-island/releases">
    <img src="https://img.shields.io/github/v/release/erha19/code-island?display_name=tag&style=flat-square" alt="Latest release">
  </a>
  <a href="https://github.com/erha19/code-island/releases">
    <img src="https://img.shields.io/github/downloads/erha19/code-island/total?style=flat-square" alt="Release downloads">
  </a>
  <img src="https://img.shields.io/badge/macOS-14%2B-0A84FF?style=flat-square&logo=apple&logoColor=white" alt="macOS 14 or later">
  <img src="https://img.shields.io/badge/Swift-6.1-FA7343?style=flat-square&logo=swift&logoColor=white" alt="Swift 6.1">
  <img src="https://img.shields.io/badge/Clients-11%2B-111827?style=flat-square" alt="Supports 11 plus client families">
  <img src="https://img.shields.io/badge/License-MIT-4F46E5?style=flat-square" alt="MIT license">
</p>

<p align="center">
  <img src="docs/images/notch-panel.png" width="480" alt="Code Island preview">
</p>


<p align="center">
  <sub>Watch active coding sessions, answer follow-up questions, and jump back to the right terminal or IDE window.</sub>
</p>

<p align="center">
  <sub>Official website: <a href="https://git.n.xiaomi.com/cloudpm/codeisland/">git.n.xiaomi.com/cloudpm/codeisland</a></sub>
</p>

<p align="center">
  <img src="docs/images/mascots/claude.gif" width="36" alt="Claude mascot" title="Claude Code">&nbsp;
  <img src="docs/images/mascots/codex.gif" width="36" alt="Codex mascot" title="Codex">&nbsp;
  <img src="docs/images/mascots/gemini.gif" width="36" alt="Gemini CLI mascot" title="Gemini CLI">&nbsp;
  <img src="docs/images/mascots/hermes.gif" width="36" alt="Hermes Agent mascot" title="Hermes Agent">&nbsp;
  <img src="docs/images/mascots/qwen.gif" width="36" alt="Qwen Code mascot" title="Qwen Code">&nbsp;
  <img src="docs/images/mascots/openclaw.gif" width="36" alt="OpenClaw mascot" title="OpenClaw">&nbsp;
  <img src="docs/images/mascots/opencode.gif" width="36" alt="OpenCode mascot" title="OpenCode">&nbsp;
  <img src="docs/images/mascots/cursor.gif" width="36" alt="Cursor mascot" title="Cursor">&nbsp;
  <img src="docs/images/mascots/qoder.gif" width="36" alt="Qoder mascot" title="Qoder">&nbsp;
  <img src="docs/images/mascots/codebuddy.gif" width="36" alt="CodeBuddy mascot" title="CodeBuddy">&nbsp;
  <img src="docs/images/mascots/copilot.gif" width="36" alt="GitHub Copilot mascot" title="GitHub Copilot">
</p>
<p align="center">
  <sub>Claude Code · Codex · Gemini CLI · Hermes Agent · Qwen Code · OpenClaw · OpenCode · Cursor · Qoder · CodeBuddy · GitHub Copilot</sub>
</p>

<a id="buddy-detach"></a>
## Buddy Detach in v0.5.0+

Starting in `v0.5.0` - the first release after `v0.4.0` - Code Island can detach the active Buddy from the notch. Press and hold the notch, drag the Buddy upward out of the notch area, and it becomes an independent floating companion that stays with you across other windows.

<p align="center">
  <img src="docs/images/code-island-0.5.0-buddy-detach.png" width="960" alt="Code Island v0.5.0 Buddy detach poster">
</p>

- **Three-step interaction** - press and hold, drag outward, then let go to keep the Buddy floating.
- **Independent floating presence** - keep session awareness visible even when you are no longer watching the top notch.
- **Free placement with low disruption** - move the Buddy where it helps without pinning it to the menu bar.
- **Same Island context** - the floating Buddy still represents the same live session, mascot identity, and progress cues.

## What is Code Island?

Code Island is a macOS menu bar app that expands into a Dynamic Island-style surface when your coding agents need attention. It listens to Claude-style hooks, Codex hooks, Gemini CLI hooks, Hermes Agent plugin hooks, Qwen Code hooks, OpenClaw internal hooks plus session transcripts, the Codex app-server, OpenCode plugins, and compatible IDE integrations so approvals, input requests, completions, and session summaries show up without babysitting terminal tabs.

If you have seen [Vibe Island](https://vibeisland.app/), Code Island is positioned as an independent open-source alternative in the same category: a native macOS notch/menu bar surface for monitoring and controlling AI coding sessions.

## Features

Code Island focuses on the moments that actually interrupt coding flow, then keeps them visible and actionable from a native macOS notch surface.

- **Attention-first UI** - Stay compact until a session needs approval, input, review, or intervention.
- **Act from the notch** - Approve tools, deny requests, and answer follow-up prompts without hunting through tabs.
- **Claude Code auto-approve** - Turn on per-session auto-approval when you want Claude Code to stop pausing on every permission request.
- **One-click return** - Jump back to the right iTerm2, Ghostty, Terminal.app, tmux pane, or IDE window.
- **SSH terminal support** - Bootstrap a remote CodeIslandBridge over SSH, rewrite the remote Claude-compatible hooks to point back at your Mac, and keep remote terminal activity visible in the same local Island UI.
- **Multi-agent coverage** - Track Claude Code, Codex, Gemini CLI, Hermes Agent, Qwen Code, OpenClaw, OpenCode, Cursor, Qoder, CodeBuddy, WorkBuddy, GitHub Copilot, and other compatible sessions in one place.
- **OpenClaw gateway support** - Follow OpenClaw sessions from managed internal hooks, then refill the conversation from OpenClaw's local session transcripts so the Island UI can show the actual back-and-forth instead of a single inbound message.
- **Codex hook + app-server sync** - Support Codex CLI hooks, live app-server threads, and rollout parsing fallback when needed.
- **Custom sounds** - Pick per-event macOS sounds or import local sound packs for your own notification style.
- **Custom agent mascots** - Give each client its own animated mascot override across the notch, session list, and hover UI.
- **Buddy detach in v0.5.0+** - Drag the active Buddy out of the notch so it can stay nearby as an independent floating companion.
- **Hermes courier-fox mascot** - Hermes Agent uses a gold courier fox with a winged helmet and satchel so plugin-hook sessions stay visually distinct from the Claude/Qwen family.
- **Qwen capybara mascot** - Qwen Code now ships with a mint-scarf capybara mascot tuned for prompt, reply, and notification-heavy flows.

<a id="supported-tools"></a>
## Supported Tools

<p align="center">
  <img src="docs/images/code-island-mascot-poster.png" width="960" alt="Code Island supported tools poster">
</p>

Code Island also ships VS Code-compatible focus extensions for VS Code, Cursor, CodeBuddy, WorkBuddy, and Qoder. `QoderWork` is hook-only today and does not participate in the IDE extension path.

Hermes Agent is integrated through a generated plugin directory at `~/.hermes/plugins/code_island/`. Hermes' gateway hook directories under `~/.hermes/hooks/` do not run in the CLI, so Code Island uses the official `ctx.register_hook()` plugin surface to observe prompt submission, tool activity, model replies, and session end events.

Qwen Code is supported as a first-class hook client through `~/.qwen/settings.json`, and its built-in mascot is the mint-scarf capybara shown in the README GIF strip. The visual is meant to feel calm and dependable, while still carrying a small Qwen-tinted scarf and reply bubble instead of another generic bird or blob.

OpenClaw is supported through a managed internal hook directory under `~/.openclaw/hooks/` plus transcript-aware session refresh from `~/.openclaw/agents/main/sessions/`. That combination lets Code Island surface OpenClaw's lightweight message hooks quickly, then backfill the full conversation from the local session log once the assistant reply lands.

SSH support is a core workflow, not a sidecar script. Code Island can bootstrap a bridge onto a remote macOS or Linux host, rewrite remote Claude-compatible and Qwen Code hook configs to use that bridge, install supported OpenClaw internal hooks on the remote host, and keep a bidirectional forwarding path back into the local menu-bar UI. That means approvals, follow-up questions, notifications, and jump-back routing from remote SSH terminals still land in the same Island surface on your Mac.

The mascot GIFs used throughout this README are generated from the live `MascotView` implementation via `./scripts/render-mascots.sh`.
The OpenClaw feature poster in `docs/images/code-island-openclaw-poster.png` is generated via `./scripts/render-openclaw-poster.sh`.

<a id="installation"></a>
## Installation

### Download a Release

1. Visit the [Git repository](https://git.n.xiaomi.com/cloudpm/codeisland/) for the product overview and latest download link, or go straight to [Releases](https://git.n.xiaomi.com/cloudpm/codeisland/releases).
2. Download the latest DMG.
3. Move `Code Island.app` into your Applications folder.
4. Launch the app and start the clients you want Code Island to monitor.

> On first launch, macOS may ask you to confirm the app or grant Accessibility / Apple Events permissions for focus features.

<a id="build-from-source"></a>
### Build from Source

Requires macOS 14+ and an Xcode toolchain that can build the Xcode project and the Swift 6.1 `Prototype` package tests.

```bash
git clone https://github.com/erha19/code-island.git
cd code-island

# Debug build
xcodebuild -project CodeIsland.xcodeproj -scheme CodeIsland -configuration Debug build

# Release build
xcodebuild -project CodeIsland.xcodeproj -scheme CodeIsland -configuration Release build
```

To create a locally shareable unsigned package for local testing:

```bash
./scripts/package-unsigned.sh
```

The script re-signs the built app bundle with a consistent ad-hoc signature before creating the `.dmg` and `.zip`, which helps embedded frameworks launch more reliably on another machine. The package is still unsigned for distribution and not notarized, so first launch may still require `Open` from Finder's context menu or manual quarantine removal.
The generated files land in `releases/unsigned/` as `CodeIsland-<version>.dmg` and `CodeIsland-<version>.zip`.
The DMG uses the repo-tracked installer artwork at `docs/images/code-island-dmg-installer-background.png` by default; set `CODE_ISLAND_DMG_BACKGROUND_SOURCE` if you want to preview a different background locally.

To create signed and notarized release packages in GitHub Actions, configure the release secrets described in [docs/sparkle-release.md](docs/sparkle-release.md) and run `.github/workflows/release-packages.yml` against a `v*` tag or the manual workflow dispatch input.

The same workflow also publishes a Linux `CodeIslandBridge` asset that Code Island can download when bootstrapping Linux SSH hosts.

For the full notarized release flow and the GitHub Releases backed Sparkle appcast setup, see [docs/sparkle-release.md](docs/sparkle-release.md).

## Testing

The fastest full-repo regression path is:

```bash
./scripts/test.sh
```

That covers:

```bash
swift test --package-path Prototype
xcodebuild -project CodeIsland.xcodeproj -scheme CodeIsland -configuration Debug CODE_SIGNING_ALLOWED=NO test -only-testing:CodeIslandTests
xcodebuild -project CodeIsland.xcodeproj -scheme CodeIsland -configuration Debug CODE_SIGN_IDENTITY=- test
```

Useful targeted slices:

```bash
swift test --package-path Prototype --filter IslandBridgeE2ETests
xcodebuild -project CodeIsland.xcodeproj -scheme CodeIsland -configuration Debug CODE_SIGNING_ALLOWED=NO test -only-testing:CodeIslandTests
xcodebuild -project CodeIsland.xcodeproj -scheme CodeIsland -configuration Debug CODE_SIGN_IDENTITY=- test -only-testing:CodeIslandUITests
```

If `CodeIslandUITests-Runner` stays suspended on macOS, run the UI tests from Xcode with a valid local signing identity and check `amfid` / `AppleSystemPolicy` logs before treating it as an app regression.

## Settings

Code Island currently ships a 4-category settings panel:

- **General** - launch at login and baseline app behavior
- **Display** - notch display target and placement behavior
- **Mascot** - client mascot previews, per-client overrides, animation states
- **Sound** - event-specific sounds, sound pack mode, sound pack import

## Custom Sounds

Code Island currently supports three sound modes under `Settings -> Sound`:

- **System sounds** - choose a macOS sound for each event.
- **Built-in 8-bit** - use Island's bundled retro sound set, including the fixed client startup sound.
- **Sound pack** - load a local OpenPeon / CESP-compatible pack from disk.

### Quick setup

1. Open `Settings -> Sound`.
2. Turn on `Enable sounds`.
3. Pick the mode you want:
   - `System sounds` if you just want a different macOS sound per event.
   - `Sound pack` if you want fully custom audio files.
4. Preview each event with the play button and leave only the event toggles you want enabled.

### Import a local sound pack

1. Switch `Sound mode` to `Sound pack`.
2. Click `Import local sound pack`.
3. Choose a folder that contains `openpeon.json`.
4. Pick the imported pack from the `Sound pack` dropdown.

Code Island also auto-discovers packs placed under `~/.openpeon/packs` and `~/.claude/hooks/peon-ping/packs`.

### Minimal sound pack layout

```text
my-pack/
  openpeon.json
  session-start.wav
  attention.ogg
  complete.mp3
  error.wav
  limit.wav
```

```json
{
  "cesp_version": "1.0",
  "name": "my-pack",
  "display_name": "My Pack",
  "categories": {
    "task.acknowledge": {
      "sounds": [{ "file": "session-start.wav", "label": "Session Start" }]
    },
    "input.required": {
      "sounds": [{ "file": "attention.ogg", "label": "Attention" }]
    },
    "task.complete": {
      "sounds": [{ "file": "complete.mp3", "label": "Complete" }]
    },
    "task.error": {
      "sounds": [{ "file": "error.wav", "label": "Error" }]
    },
    "resource.limit": {
      "sounds": [{ "file": "limit.wav", "label": "Limit" }]
    }
  }
}
```

### Event mapping

- `Processing started` checks `task.acknowledge`, then `session.start`.
- `Attention required` checks `input.required`.
- `Task completed` checks `task.complete`.
- `Task error` checks `task.error`.
- `Resource limit` checks `resource.limit`.

Release builds can also publish a Linux `CodeIslandBridge` artifact alongside the macOS app packages, which Code Island uses when bootstrapping remote SSH hosts that are not running macOS.

Sound packs can use `.wav`, `.mp3`, or `.ogg` files. If a selected pack does not provide a matching category for an event, Code Island falls back to the macOS system sound selected for that event.

## How It Works

```text
Claude / Codex / Gemini CLI / OpenCode / Cursor / Qoder / CodeBuddy / WorkBuddy / Copilot / ...
  -> hook or app-server event
    -> Code Island monitor + normalization layer
      -> SessionStore
        -> SessionMonitor / NotchViewModel
          -> notch, list, hover preview, completion popup
```

Implementation details worth knowing:

- Claude-family tools enter through managed hook files plus the embedded `CodeIslandBridge` launcher.
- Codex sessions can come from hook events or the live `codex app-server` websocket monitor.
- Gemini CLI hooks are installed into `~/.gemini/settings.json`; tool matchers use Gemini's regex-based hook matcher syntax.
- Qwen Code hooks are installed into `~/.qwen/settings.json`; the bridge follows the official event names and uses `Stop` / `SessionEnd` / `Notification` messages to surface popup-ready summaries in Island.
- OpenCode is wired through a generated plugin file under `~/.config/opencode/plugins/`.
- Remote SSH hosts can bootstrap `CodeIslandBridge`, rewrite remote Claude-compatible hooks to target that bridge, and forward remote events back into the local Code Island UI.
- Focus routing spans iTerm2, Ghostty, Terminal.app, tmux, and VS Code-compatible IDE extensions.

## Requirements

- macOS 14.0 or later
- Best experience on MacBooks with a notch, but external displays are supported too
- Install whichever CLI or desktop clients you want Code Island to monitor

## Acknowledgments

Code Island is a fork of [ping-island](https://github.com/erha19/ping-island/), adapted for internal decoupling and iteration. We gratefully acknowledge the original project and its contributors.

Code Island also follows the lineage of notch-first agent monitors such as [claude-island](https://github.com/farouqaldori/claude-island), and adapts that idea into a broader multi-client session surface with hooks, app-server sync, and IDE routing.

## License

MIT - see [LICENSE.md](LICENSE.md).

This project is derived from [ping-island](https://github.com/erha19/ping-island/) (Apache 2.0) and has been relicensed under MIT for internal use.
