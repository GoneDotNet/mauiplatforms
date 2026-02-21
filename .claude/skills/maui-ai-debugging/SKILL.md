---
name: maui-ai-debugging
description: >
  End-to-end workflow for building, deploying, inspecting, and debugging .NET MAUI and MAUI Blazor Hybrid apps
  as an AI agent. Use when: (1) Building or running a MAUI app on iOS simulator, Android emulator, Mac Catalyst,
  or Linux/GTK, (2) Inspecting or interacting with a running app's UI (visual tree, tapping, filling text,
  screenshots, property queries), (3) Debugging Blazor WebView content via CDP, (4) Managing simulators or
  emulators, (5) Setting up MauiDevFlow in a MAUI project, (6) Completing a build-deploy-inspect-fix feedback
  loop, (7) Handling permission dialogs and system alerts, (8) Managing multiple simultaneous apps via the
  broker daemon. Covers: maui-devflow CLI, androidsdk.tool, appledev.tools, adb, xcrun simctl, xdotool,
  and dotnet build/run for all MAUI target platforms including Linux/GTK.
---

# MAUI AI Debugging

Build, deploy, inspect, and debug .NET MAUI apps from the terminal. This skill enables a complete
feedback loop: **build → deploy → inspect → fix → rebuild**.

## Prerequisites

```bash
dotnet tool install --global Redth.MauiDevFlow.CLI || dotnet tool update --global Redth.MauiDevFlow.CLI
dotnet tool install --global androidsdk.tool    # Android only
dotnet tool install --global appledev.tools     # iOS/Mac only
```

Keep the skill up to date: `maui-devflow update-skill`. For full update and version checking
procedures, see [references/setup.md](references/setup.md#checking-for-updates).

## Integrating MauiDevFlow into a MAUI App

For complete setup instructions, see [references/setup.md](references/setup.md).

**Quick summary:**
1. Add NuGet packages (`Redth.MauiDevFlow.Agent`, and `Redth.MauiDevFlow.Blazor` for Blazor Hybrid)
   - For **Linux/GTK apps** (detected via `grep -i 'GirCore\|Maui\.Gtk' *.csproj`), use `Agent.Gtk` and `Blazor.Gtk` instead
2. Register in `MauiProgram.cs` inside `#if DEBUG`
3. For Blazor Hybrid: chobitsu.js is auto-injected (no manual script tag needed)
4. For Mac Catalyst: ensure `network.server` entitlement
5. For Android: run `adb reverse` for broker + agent ports
6. For Linux: no special network setup needed (direct localhost)

## Core Workflow

### 1. Ensure a Device/Simulator/Emulator is Running

**iOS Simulator:**
```bash
xcrun simctl list devices booted                              # check booted sims
xcrun simctl boot <UDID>                                      # boot if needed
```

**Android Emulator:**
```bash
android avd list                                              # list AVDs
android avd start --name <avd-name>                           # start emulator
```

**Mac Catalyst / Linux/GTK:** No device setup needed — runs as desktop app.

### 2. Detect the TFM

**IMPORTANT:** Before building, detect the correct Target Framework Moniker from the project.
Do NOT assume `net10.0` — many projects use `net9.0`, `net8.0`, etc.

```bash
grep -i 'TargetFrameworks' *.csproj Directory.Build.props 2>/dev/null
```

Use the detected version (e.g. `net9.0`) in all build commands. The examples use `$TFM`.

### 3. Build and Deploy

**CRITICAL:** `dotnet build -t:Run` **blocks until the app exits**. You MUST launch it
asynchronously and then poll for the app to be ready. Do NOT wait for the process to finish —
it never will (until the app is closed).

**Correct launch pattern:**
1. **Kill the old app first** — if a previous instance is running, stop it before rebuilding.
   Otherwise the old app's agent will still be registered with the broker, and polling
   `maui-devflow MAUI status` will succeed against the stale instance instead of the new build.
   - Stop the previous async shell (if you still have it), or kill the app process directly
   - Verify with `maui-devflow list` that the old agent is gone before launching
2. Start `dotnet build -t:Run` in an **async/background shell** (e.g., `mode: "async"`)
3. Read output from the async shell periodically to watch for build completion / app launch
4. Poll `maui-devflow MAUI status` (or `maui-devflow list`) until the agent connects
5. If the agent doesn't appear after ~60-90 seconds, check the async shell output for build errors

```bash
# iOS Simulator (run in async shell)
dotnet build -f $TFM-ios -t:Run -p:_DeviceName=:v2:udid=<UDID>

# Android Emulator (run in async shell)
dotnet build -f $TFM-android -t:Run

# Mac Catalyst (run in async shell)
dotnet build -f $TFM-maccatalyst -t:Run

# Linux/GTK (run in async shell)
dotnet run --project <path-to-gtk-project>
```

Build + Run can take 30-120+ seconds. Use `initial_wait: 120` or higher for async monitoring.

**Device/simulator compatibility:** The TFM compile target does NOT mean you need a matching
emulator/simulator version. Apps run on any device at or above `SupportedOSPlatformVersion`.
Use whatever emulator/simulator is available.

For Android emulators, set up port forwarding after deploy:
```bash
adb reverse tcp:19223 tcp:19223  # Broker (required — lets agent register with host broker)
adb forward tcp:<port> tcp:<port> # Agent (required — lets CLI reach agent in emulator)
```

### 4. Verify Connectivity

After launching the app asynchronously, wait for the agent to connect:

```bash
maui-devflow wait                 # Block until any agent connects (default 120s timeout)
maui-devflow wait --project path/to/App.csproj  # Wait for a specific project's agent
maui-devflow wait --json          # Output full agent info as JSON
maui-devflow list                 # Show all registered agents (via broker)
maui-devflow MAUI status          # Agent connection + CDP readiness
```

Use `maui-devflow wait` instead of polling — it blocks until an agent registers with the broker
and prints the assigned port. With `--project`, it filters to a specific app. If no agent connects
within the timeout (default 120s), it exits with code 1.

The `list` command shows all agents registered with the broker, including their platform,
TFM, and assigned port. Use this to find the port for `--agent-port` when multiple apps run.

### 5. Inspect and Interact

**Typical inspection flow:**
1. `maui-devflow MAUI tree` — see the full visual tree with element IDs, types, text, bounds
2. `maui-devflow MAUI query --automationId "MyButton"` — find specific elements
3. `maui-devflow MAUI element <id>` — get full details (type, bounds, visibility, children)
4. `maui-devflow MAUI property <id> Text` — read any property by name
5. `maui-devflow MAUI screenshot --output screen.png` — visual verification

**Property inspection** is more reliable than screenshots for verifying exact runtime values:
```bash
maui-devflow MAUI property <id> BackgroundColor    # verify dark mode colors
maui-devflow MAUI property <id> IsVisible          # check element visibility
```

**Live editing (no rebuild needed):**
```bash
maui-devflow MAUI set-property <id> TextColor "Tomato"
maui-devflow MAUI set-property <id> FontSize "24"
```
Supports: string, bool, int, double, Color (named/hex), Thickness, enums. Changes persist
until the app restarts — safe for experimentation.

**Typical interaction flow:**
1. `maui-devflow MAUI fill <entryId> "text"` — type into Entry/Editor fields
2. `maui-devflow MAUI tap <buttonId>` — tap buttons, checkboxes, list items
3. `maui-devflow MAUI clear <entryId>` — clear text fields
4. Take screenshot to verify result

**Blazor WebView (if applicable):**
1. `maui-devflow cdp snapshot` — DOM tree as accessible text (best for AI)
2. `maui-devflow cdp Input fill "css-selector" "text"` — fill inputs
3. `maui-devflow cdp Input dispatchClickEvent "css-selector"` — click elements
4. `maui-devflow cdp Runtime evaluate "js-expression"` — run JS

**Live CSS/DOM editing in Blazor (no rebuild needed):**
```bash
maui-devflow cdp Runtime evaluate "document.querySelector('h1').style.color = 'tomato'"
maui-devflow cdp Runtime evaluate "document.documentElement.style.setProperty('--bg-color', '#1a1a2e')"
```

### 6. Reading Application Logs

MauiDevFlow automatically captures all `ILogger` output and WebView `console.*` calls
to rotating log files, retrievable remotely:

```bash
maui-devflow MAUI logs                   # fetch 100 most recent log entries
maui-devflow MAUI logs --limit 50        # fetch 50 entries
maui-devflow MAUI logs --source webview  # only WebView/Blazor console logs
maui-devflow MAUI logs --source native   # only native ILogger logs
```

**Debugging workflow:** Reproduce the issue → `maui-devflow MAUI logs --limit 20` → check for
errors. Add temporary `ILogger` calls for more detail, rebuild, reproduce, and fetch logs again.

### 7. Screen Recording

Capture video of the app while performing interactions. Recording is host-side (not in-app)
using platform-native tools.

```bash
# Start recording (default 30s timeout)
maui-devflow MAUI recording start --output demo.mp4

# Interact with the app
maui-devflow MAUI tap <buttonId>
maui-devflow MAUI navigate "//blazor"
maui-devflow MAUI fill <entryId> "Hello World"

# Stop and save
maui-devflow MAUI recording stop
```

**Platform tools used automatically:**
- **Android:** `adb screenrecord` (max 180s, capped with warning)
- **iOS Simulator:** `xcrun simctl io recordVideo`
- **Mac Catalyst:** `screencapture -v` (targets app window when possible)
- **Windows/Linux:** `ffmpeg` (must be on PATH)

**Options:** `--timeout <seconds>` (default 30), `--output <path>` (default `recording_<timestamp>.mp4`).
Only one recording at a time — stop before starting a new one.

### 8. Rebuild

**Always kill the running app before rebuilding.** If the old instance is still running, its
agent stays registered with the broker — polling `maui-devflow MAUI status` will succeed against
the stale app instead of waiting for the new build.

1. Stop the previous async shell or kill the app process
2. Verify with `maui-devflow list` that the old agent is gone
3. Run `dotnet build -f $TFM-<platform> -t:Run ...` in an async shell
4. `maui-devflow wait` until connected → inspect

If the build fails, see [references/troubleshooting.md](references/troubleshooting.md).

## Command Reference

### maui-devflow MAUI (Native Agent)

Global options: `--agent-host` (default localhost), `--agent-port` (auto-discovered via broker), `--platform`.

These options work on any subcommand position: `maui-devflow MAUI status --agent-port 10224`
or `maui-devflow --agent-port 10224 MAUI status` — both are valid.

| Command | Description |
|---------|-------------|
| `MAUI status` | Agent connection status, platform, app name |
| `MAUI tree [--depth N]` | Visual tree (IDs, types, text, bounds). Depth 0=unlimited |
| `MAUI query --type T --automationId A --text T` | Find elements (any/all filters) |
| `MAUI tap <elementId>` | Tap an element |
| `MAUI fill <elementId> <text>` | Fill text into Entry/Editor |
| `MAUI clear <elementId>` | Clear text from element |
| `MAUI screenshot [--output path.png]` | PNG screenshot |
| `MAUI property <elementId> <prop>` | Read property (Text, IsVisible, FontSize, etc.) |
| `MAUI set-property <elementId> <prop> <value>` | Set property (live editing — colors, text, sizes, etc.) |
| `MAUI element <elementId>` | Full element JSON (type, bounds, children, etc.) |
| `MAUI navigate <route>` | Shell navigation (e.g. `//native`, `//blazor`) |
| `MAUI logs [--limit N] [--skip N] [--source S]` | Fetch application logs (newest first). Source: native, webview, or omit for all |
| `MAUI recording start [--output path] [--timeout 30]` | Start screen recording. Default timeout 30s. Uses platform-native tools (adb screenrecord, xcrun simctl, screencapture, ffmpeg) |
| `MAUI recording stop` | Stop active recording and save the video file |
| `MAUI recording status` | Check if a recording is currently in progress |

Element IDs come from `MAUI tree` or `MAUI query`. AutomationId-based elements use their
AutomationId directly. Others use generated hex IDs. When multiple elements share the same
AutomationId, suffixes are appended: `TodoCheckBox`, `TodoCheckBox_1`, `TodoCheckBox_2`, etc.

### maui-devflow cdp (Blazor WebView CDP)

Global options: `--agent-host` (default localhost), `--agent-port` (auto-discovered via broker).
CDP commands use the same agent port — all communication goes through a single port.

| Command | Description |
|---------|-------------|
| `cdp status` | CDP connection status |
| `cdp snapshot` | Accessible DOM text (best for AI agents) |
| `cdp Browser getVersion` | Browser/WebView version info |
| `cdp Runtime evaluate <expr>` | Evaluate JavaScript |
| `cdp DOM getDocument` | Full DOM document |
| `cdp DOM querySelector <sel>` | Find first matching element |
| `cdp DOM querySelectorAll <sel>` | Find all matching elements |
| `cdp DOM getOuterHTML <sel>` | Get outer HTML of element |
| `cdp Page navigate <url>` | Navigate to URL |
| `cdp Page reload` | Reload page |
| `cdp Page captureScreenshot` | Screenshot as base64 |
| `cdp Input dispatchClickEvent <sel>` | Click element by CSS selector |
| `cdp Input insertText <text>` | Insert text at focused element |
| `cdp Input fill <selector> <text>` | Focus + fill text into element |

### maui-devflow Broker & Discovery

The broker is a background daemon that manages port assignments for all running agents.
The CLI auto-starts the broker on first use — no manual setup needed.

| Command | Description |
|---------|-------------|
| `list` | Show all registered agents (ID, app, platform, TFM, port, uptime) |
| `wait [--timeout 120] [--project path] [--wait-platform P] [--json]` | Wait for an agent to connect. Outputs the port (or JSON with `--json`). Useful after `dotnet build -t:Run` to block until the app is ready |
| `broker status` | Broker daemon status and connected agent count |
| `broker start` | Start broker daemon (auto-started by CLI — rarely needed manually) |
| `broker stop` | Stop broker daemon |
| `broker log` | Show broker log file |

### maui-devflow batch (Multi-Command Execution)

Execute multiple MAUI/cdp commands in a single CLI invocation via stdin. Outputs JSONL
responses (one JSON object per line) to stdout — ideal for AI agents and scripting.

```bash
# Pipe multiple commands (semicolons or newlines as separators)
echo "MAUI fill textUsername user; MAUI fill textPassword pwd123; MAUI tap buttonLogin" | maui-devflow batch

# Multi-line input
printf "MAUI status\nMAUI tree\nMAUI screenshot --output screen.png" | maui-devflow batch

# With options
echo "MAUI status; MAUI tree" | maui-devflow batch --delay 500 --continue-on-error --agent-port 10224

# Human-readable output instead of JSONL
echo "MAUI status; MAUI tree" | maui-devflow batch --human
```

| Option | Default | Description |
|--------|---------|-------------|
| `--delay <ms>` | 250 | Delay between commands (lets UI settle) |
| `--continue-on-error` | false | Continue after a command fails (default: stop) |
| `--human` | false | Human-readable output instead of JSONL |

**JSONL response format** (one per command, streamed as each completes):
```json
{"command":"MAUI fill textUsername user","exit_code":0,"output":"Filled: textUsername"}
{"command":"MAUI tap buttonLogin","exit_code":1,"output":"Error: Element not found: buttonLogin"}
```

**Interactive streaming:** The batch command processes stdin line-by-line, so a caller can
read each JSONL response before sending the next command. This enables reactive workflows
where the AI agent inspects results and decides the next action.

**Input rules:**
- Lines starting with `#` are comments (skipped)
- Empty lines are skipped
- Semicolons separate multiple commands on one line
- Quoted strings are preserved: `MAUI fill myEntry "hello world"`
- Only `MAUI` and `cdp` commands are allowed (broker/list/etc. are rejected)

**How port discovery works:** When you run any `MAUI` or `cdp` command, the CLI:
1. Auto-starts the broker if not running
2. Queries the broker for agents matching the current project (`.csproj` in cwd)
3. If one agent matches → uses its port automatically
4. If multiple match → prints a disambiguation table to stderr
5. Falls back to `.mauidevflow` config file → default 9223

**Multiple apps simultaneously:** The broker assigns unique ports from range 10223–10899.
Use `maui-devflow list` to see all agents, then target a specific one:
```bash
maui-devflow MAUI status --agent-port 10224    # target specific agent
```

## Platform Details

For detailed platform-specific setup, simulator/emulator management, and troubleshooting:

- **Setup & Installation**: See [references/setup.md](references/setup.md)
- **iOS / Mac Catalyst**: See [references/ios-and-mac.md](references/ios-and-mac.md)
- **Android**: See [references/android.md](references/android.md)
- **Linux / GTK**: See [references/linux.md](references/linux.md)
- **Troubleshooting**: See [references/troubleshooting.md](references/troubleshooting.md)

## Multi-Project / Custom Ports

**With the broker (recommended):** The broker automatically assigns ports to agents from
range 10223–10899. No manual port configuration needed — just build and run your apps.
Use `maui-devflow list` to see assigned ports. The CLI auto-discovers the right agent
when run from the project directory.

**Legacy `.mauidevflow` config (fallback):** If the broker isn't available:
```json
{ "port": 9225 }
```

**Port priority:** Explicit `--agent-port` > Broker discovery > `.mauidevflow` config > Default 9223.

## Tips

- **Use `maui-devflow batch` for multi-step interactions** — instead of running N separate CLI
  invocations, pipe multiple commands through batch mode. This resolves the agent port once,
  adds automatic delays between commands, and returns structured JSONL responses.
- **Always use `maui-devflow MAUI screenshot` or `maui-devflow cdp Page captureScreenshot`** for
  screenshots. These capture the app's UI in-process from the rendering layer — the app does NOT
  need to be in the foreground or focused. Never use `osascript` to bring windows to the front
  for screenshots; it's unnecessary and unreliable.
- **Avoid `osascript` unless absolutely necessary.** The `maui-devflow` CLI provides commands for
  nearly everything: screenshots, tapping, text input, navigation, and property inspection. Only
  use `osascript` for OS-level operations that `maui-devflow` cannot do (e.g., toggling dark mode,
  dismissing macOS crash-recovery dialogs).
- Use `AutomationId` on important MAUI controls for stable element references.
- The visual tree only reflects what's currently rendered. Off-screen items in CollectionView
  may not appear until scrolled into view.
- For Blazor Hybrid, `cdp snapshot` is the most AI-friendly way to read page state.
- Build times: Mac Catalyst ~5-10s, iOS ~30-60s, Android ~30-90s, Linux/GTK ~5-10s.
- After Android deploy, always run `adb reverse tcp:19223` for broker + `adb forward` for agent.
- Both MAUI native and CDP commands share a single port — no separate WebSocket endpoint.
