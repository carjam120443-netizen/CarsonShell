# CarsonShell

A modern, customizable Windows shell built in C# and .NET. CarsonShell has its own command parser, built-in commands, aliases, themes, and explicit PowerShell/CMD interoperability.

> **Status:** Early development / MVP. Automated builds are prereleases and may contain bugs.

## Download

Open [GitHub Releases](https://github.com/carjam120443-netizen/CarsonShell/releases) and download the newest build:

- **CarsonShell-Portable-win-x64.zip** — portable build. Extract it anywhere and run `CarsonShell.exe`. No installer or separate .NET runtime is required. Keep the extracted folder together if additional files are added in future versions.
- **CarsonShell-Setup-win-x64.exe** — per-user Windows setup installer. Installs under your local user profile by default, adds a Start Menu shortcut, and offers an optional desktop shortcut. It does not require administrator access by default.

These are automated **prerelease** builds created from updates to `main`. They are not stable releases yet. Windows may show a SmartScreen warning because the installer and executable are not code-signed.

## Goals

- A shell engine owned by CarsonShell—not just a customized PowerShell prompt.
- A predictable quoted-argument parser.
- Useful built-ins for navigation, aliases, help, and themes.
- Explicit ways to invoke PowerShell, CMD, and external executables.
- Portable, user-level operation by default; no administrator access required for the shell itself.
- Tests and automated Windows builds as the project grows.

## Requirements

- Windows 10 or Windows 11, x64
- For prebuilt downloads: no separate .NET runtime required
- For building from source: .NET 8 SDK or newer SDK capable of targeting `net8.0`

## Build and run from source

```powershell
dotnet build CarsonShell.sln
dotnet run --project src/CarsonShell
```

Publish a self-contained Windows x64 build:

```powershell
dotnet publish src/CarsonShell/CarsonShell.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Built-in commands

| Command | Purpose |
|---|---|
| `help` | List commands |
| `clear` | Clear the screen |
| `exit` | Exit CarsonShell |
| `pwd` | Print the current directory |
| `cd [path]` | Change directory |
| `ls [path]` | List files and directories |
| `alias` | List aliases |
| `alias name = command` | Define an in-session alias |
| `unalias name` | Remove an alias |
| `theme [name]` | Show or change the color theme |
| `exec program [args...]` | Run an executable with separate arguments |
| `ps command...` | Run a command through Windows PowerShell |
| `cmd command...` | Run a command through CMD |

Themes in the MVP: `default`, `cyberpunk`, `nord`, and `dracula`.

### Examples

```text
CarsonShell> pwd
CarsonShell> cd "D:\\My Projects"
CarsonShell> alias ll = ls
CarsonShell> theme cyberpunk
CarsonShell> ps Get-ComputerInfo
CarsonShell> cmd ver
CarsonShell> exec git --version
```

Aliases and theme selection are currently session-only. Persistent configuration, pipes, redirection, scripting, and command completion are planned features—not yet implemented.

## Build pipeline

Every push to `main` builds the project, creates a portable ZIP and a Windows setup installer, uploads both as 90-day Actions artifacts, and publishes them as assets on a uniquely tagged GitHub prerelease. Pull requests run the build check without publishing release assets.

GitHub's **Packages** page is a separate registry for formats such as NuGet and container images; it is not where ordinary Windows EXE downloads are listed. Downloadable application builds are published under [Releases](https://github.com/carjam120443-netizen/CarsonShell/releases).

## Design and safety

CarsonShell built-ins are handled by its own dispatcher. External processes are launched only when requested through `exec`, `ps`, or `cmd`; PowerShell and CMD command strings are interpreted by those shells and should be treated accordingly. Run CarsonShell as a normal user and only execute commands you trust.

## Roadmap

- [x] Initial project and shell loop
- [x] Built-in command dispatcher
- [x] Quoted argument parsing
- [x] In-session aliases and themes
- [x] Explicit PowerShell/CMD integration
- [x] Automated Windows x64 portable ZIP and setup installer
- [x] Automated prereleases on updates to `main`
- [ ] Unit tests for parser and command behavior
- [ ] Persistent configuration and command history
- [ ] Pipes, redirection, and scripting

## License

MIT. See [LICENSE](LICENSE).
