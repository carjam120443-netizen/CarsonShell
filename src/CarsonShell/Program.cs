using System.Diagnostics;
using System.Text;

namespace CarsonShell;

internal static class Program
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase);
    private static string _theme = "default";
    private static bool _running = true;
    private static string _currentDirectory = Environment.CurrentDirectory;

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        ApplyTheme(_theme);
        Console.WriteLine("CarsonShell 0.1.0-dev — type 'help' for commands.");
        Console.WriteLine("Independent shell engine · Windows integration available\n");

        while (_running)
        {
            DrawPrompt();
            var input = Console.ReadLine();
            if (input is null) break;
            if (string.IsNullOrWhiteSpace(input)) continue;

            try
            {
                Execute(input);
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);
            }
        }

        return 0;
    }

    private static void DrawPrompt()
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = _theme == "cyberpunk" ? ConsoleColor.Magenta : ConsoleColor.Cyan;
        Console.Write("CarsonShell");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" ");
        Console.Write(_currentDirectory);
        Console.ForegroundColor = previous;
        Console.Write("> ");
    }

    private static void Execute(string input)
    {
        var tokens = Tokenize(input);
        if (tokens.Count == 0) return;

        // Expand aliases on the command name only; limit depth to avoid alias loops.
        for (var i = 0; i < 16 && Aliases.TryGetValue(tokens[0], out var expansion); i++)
        {
            var expanded = Tokenize(expansion);
            if (expanded.Count == 0) break;
            tokens = expanded.Concat(tokens.Skip(1)).ToList();
        }

        var command = tokens[0].ToLowerInvariant();
        var args = tokens.Skip(1).ToList();
        switch (command)
        {
            case "help": ShowHelp(); break;
            case "clear": case "cls": Console.Clear(); break;
            case "exit": _running = false; break;
            case "pwd": Console.WriteLine(_currentDirectory); break;
            case "cd": ChangeDirectory(args); break;
            case "ls": case "dir": ListDirectory(args); break;
            case "alias": ManageAlias(input, args); break;
            case "unalias": RemoveAlias(args); break;
            case "theme": SetTheme(args); break;
            case "exec": RunExecutable(args); break;
            case "ps": RunShell("powershell.exe", args); break;
            case "cmd": RunShell("cmd.exe", args); break;
            default:
                WriteError($"Unknown CarsonShell command '{tokens[0]}'. Try 'help'. Use 'exec' for a program, 'ps' for PowerShell, or 'cmd' for CMD.");
                break;
        }
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        char quote = '\0';
        bool tokenStarted = false;

        foreach (char c in input)
        {
            if (quote != '\0')
            {
                if (c == quote) quote = '\0';
                else current.Append(c);
                tokenStarted = true;
                continue;
            }
            if (c == '\'' || c == '"')
            {
                quote = c;
                tokenStarted = true;
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                if (tokenStarted)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                    tokenStarted = false;
                }
                continue;
            }
            current.Append(c);
            tokenStarted = true;
        }

        if (quote != '\0') throw new FormatException("Unclosed quote in command.");
        if (tokenStarted) tokens.Add(current.ToString());
        return tokens;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("CarsonShell built-in commands:");
        Console.WriteLine("  help                    Show this help");
        Console.WriteLine("  clear | cls             Clear the screen");
        Console.WriteLine("  exit                    Exit CarsonShell");
        Console.WriteLine("  pwd                     Print current directory");
        Console.WriteLine("  cd [path]               Change directory");
        Console.WriteLine("  ls [path]               List directory contents");
        Console.WriteLine("  alias                   List aliases");
        Console.WriteLine("  alias name = command    Define an in-session alias");
        Console.WriteLine("  unalias name            Remove an alias");
        Console.WriteLine("  theme [name]            Show/set theme: default, cyberpunk, nord, dracula");
        Console.WriteLine("  exec program [args...]  Run an executable with argument-safe process launching");
        Console.WriteLine("  ps command...           Run a command through Windows PowerShell");
        Console.WriteLine("  cmd command...          Run a command through CMD");
        Console.WriteLine();
        Console.WriteLine("Aliases and themes are session-only in this MVP.");
    }

    private static void ChangeDirectory(List<string> args)
    {
        if (args.Count > 1) { WriteError("Usage: cd [path]"); return; }
        var target = args.Count == 0 ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) : ResolvePath(args[0]);
        if (!Directory.Exists(target)) { WriteError($"Directory not found: {target}"); return; }
        _currentDirectory = Path.GetFullPath(target);
        Environment.CurrentDirectory = _currentDirectory;
    }

    private static void ListDirectory(List<string> args)
    {
        if (args.Count > 1) { WriteError("Usage: ls [path]"); return; }
        var target = args.Count == 0 ? _currentDirectory : ResolvePath(args[0]);
        if (!Directory.Exists(target)) { WriteError($"Directory not found: {target}"); return; }
        foreach (var entry in Directory.GetFileSystemEntries(target).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            bool isDirectory = Directory.Exists(entry);
            var old = Console.ForegroundColor;
            Console.ForegroundColor = isDirectory ? ConsoleColor.Cyan : ConsoleColor.Gray;
            Console.WriteLine(isDirectory ? $"[DIR]  {Path.GetFileName(entry)}" : $"       {Path.GetFileName(entry)}");
            Console.ForegroundColor = old;
        }
    }

    private static void ManageAlias(string input, List<string> args)
    {
        if (args.Count == 0)
        {
            if (Aliases.Count == 0) { Console.WriteLine("No aliases defined."); return; }
            foreach (var pair in Aliases.OrderBy(x => x.Key)) Console.WriteLine($"{pair.Key} = {pair.Value}");
            return;
        }

        var equals = input.IndexOf('=');
        if (equals < 0) { WriteError("Usage: alias name = command"); return; }
        var left = input[..equals].Trim();
        var leftTokens = Tokenize(left);
        if (leftTokens.Count != 2 || !leftTokens[0].Equals("alias", StringComparison.OrdinalIgnoreCase))
        { WriteError("Usage: alias name = command"); return; }
        var name = leftTokens[1];
        var value = input[(equals + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(value)) { WriteError("Alias command cannot be empty."); return; }
        if (name.Equals("alias", StringComparison.OrdinalIgnoreCase) || name.Equals("unalias", StringComparison.OrdinalIgnoreCase))
        { WriteError("That alias name is reserved."); return; }
        Aliases[name] = value;
        Console.WriteLine($"Alias set: {name} = {value}");
    }

    private static void RemoveAlias(List<string> args)
    {
        if (args.Count != 1) { WriteError("Usage: unalias name"); return; }
        Console.WriteLine(Aliases.Remove(args[0]) ? $"Removed alias '{args[0]}'." : $"Alias '{args[0]}' was not found.");
    }

    private static void SetTheme(List<string> args)
    {
        if (args.Count == 0) { Console.WriteLine($"Current theme: {_theme}"); return; }
        if (args.Count != 1) { WriteError("Usage: theme [default|cyberpunk|nord|dracula]"); return; }
        var requested = args[0].ToLowerInvariant();
        if (requested is not ("default" or "cyberpunk" or "nord" or "dracula"))
        { WriteError("Unknown theme. Choose default, cyberpunk, nord, or dracula."); return; }
        _theme = requested;
        ApplyTheme(_theme);
        Console.WriteLine($"Theme set to {_theme}.");
    }

    private static void ApplyTheme(string theme)
    {
        (ConsoleColor foreground, ConsoleColor background) = theme switch
        {
            "cyberpunk" => (ConsoleColor.Magenta, ConsoleColor.Black),
            "nord" => (ConsoleColor.Cyan, ConsoleColor.DarkBlue),
            "dracula" => (ConsoleColor.Magenta, ConsoleColor.Black),
            _ => (ConsoleColor.Gray, ConsoleColor.Black)
        };
        try { Console.ForegroundColor = foreground; Console.BackgroundColor = background; }
        catch (IOException) { /* Some redirected terminals do not support color changes. */ }
    }

    private static void RunExecutable(List<string> args)
    {
        if (args.Count == 0) { WriteError("Usage: exec program [args...]"); return; }
        var start = new ProcessStartInfo
        {
            FileName = args[0],
            WorkingDirectory = _currentDirectory,
            UseShellExecute = false
        };
        foreach (var arg in args.Skip(1)) start.ArgumentList.Add(arg);
        try
        {
            using var process = Process.Start(start);
            if (process is null) { WriteError("Could not start process."); return; }
            process.WaitForExit();
            if (process.ExitCode != 0) Console.WriteLine($"[exit code: {process.ExitCode}]");
        }
        catch (System.ComponentModel.Win32Exception ex) { WriteError($"Could not start '{args[0]}': {ex.Message}"); }
    }

    private static void RunShell(string executable, List<string> args)
    {
        if (args.Count == 0) { WriteError($"Usage: {(executable.StartsWith("cmd", StringComparison.OrdinalIgnoreCase) ? "cmd" : "ps")} command..."); return; }
        var commandText = string.Join(' ', args);
        var start = new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = _currentDirectory,
            UseShellExecute = false
        };
        if (executable.Equals("cmd.exe", StringComparison.OrdinalIgnoreCase))
        {
            start.ArgumentList.Add("/d");
            start.ArgumentList.Add("/s");
            start.ArgumentList.Add("/c");
            start.ArgumentList.Add(commandText);
        }
        else
        {
            start.ArgumentList.Add("-NoLogo");
            start.ArgumentList.Add("-NoProfile");
            start.ArgumentList.Add("-Command");
            start.ArgumentList.Add(commandText);
        }
        try
        {
            using var process = Process.Start(start);
            if (process is null) { WriteError("Could not start shell process."); return; }
            process.WaitForExit();
            if (process.ExitCode != 0) Console.WriteLine($"[exit code: {process.ExitCode}]");
        }
        catch (System.ComponentModel.Win32Exception ex) { WriteError($"Could not start {executable}: {ex.Message}"); }
    }

    private static string ResolvePath(string path) => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(_currentDirectory, path));

    private static void WriteError(string message)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"error: {message}");
        Console.ForegroundColor = old;
    }
}
