# CarsonShell

A modern, customizable Windows shell built in C# and .NET. CarsonShell has its own command parser, built-in commands, aliases, themes, and explicit PowerShell/CMD interoperability.

> **Status:** Early development / MVP. Commands and configuration may change.

## Goals

- A shell engine owned by CarsonShell—not just a customized PowerShell prompt.
- A predictable quoted-argument parser.
- Useful built-ins for navigation, aliases, help, and themes.
- Explicit ways to invoke PowerShell, CMD, and external executables.
- Portable, user-level operation by default; no administrator access required for the shell itself.
- Tests and automated Windows builds as the project grows.

## Requirements

- Windows 10 or Windows 11
- .NET 8 SDK or newer SDK capable of targeting `net8.0`

## Build and run

```powershell
dotnet build
dotnet run --project src/CarsonShell
```

Or publish a self-contained Windows x64 build:

```powershell
dotnet publish src/CarsonShell/CarsonShell.csproj -c Release -r win-x64 --self-contained true
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

## Design and safety

CarsonShell built-ins are handled by its own dispatcher. External processes are launched only when requested through `exec`, `ps`, or `cmd`; PowerShell and CMD command strings are interpreted by those shells and should be treated accordingly. Run CarsonShell as a normal user and only execute commands you trust.

## Roadmap

- [x] Initial project and shell loop
- [x] Built-in command dispatcher
- [x] Quoted argument parsing
- [x] In-session aliases and themes
- [x] Explicit PowerShell/CMD integration
- [ ] Unit tests for parser and command behavior
- [ ] Persistent configuration and command history
- [ ] CI builds and versioned releases
- [ ] Pipes, redirection, and scripting

## License

MIT. See [LICENSE](LICENSE).
