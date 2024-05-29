#nullable disable warnings

using ConsoleTools;

namespace LunaPlayer;
public static partial class Program
{
    public const string Version = "0.4.0";
    private readonly static Dictionary<string, string> usages = [];
    private readonly static Dictionary<string, Command> commands = [];
    private delegate void Command(string[] param);

    private static void Main(string[] arg)
    {
        CommandsInit();
        usages.Add("play", "lunaplayer play /path:(path) [/volume:(vol)] [/noui] [/loop[:count]]");
        usages.Add("list-play", "lunaplayer list-play /path:(path)");
        usages.Add("explorer", "lunaplayer explorer");

        Task.Run(() =>
        {
            while (Console.WindowHeight != 1);
            Environment.Exit(15_6_20);
        });

        if (arg.Length == 0)
        {
            commands["explorer"]([]);
            return;
        }
        CommandParse(arg[0], arg[1..]);
    }

    public static void Error(string message)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[error]: "+message);
        Console.ResetColor();
        Console.CursorVisible = true;
        Environment.Exit(1);
    }
    private static void Usage()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Usage:");
        Console.WriteLine("lunaplayer <command> [params]");
        Console.ResetColor();
        Environment.Exit(1);
    }

    private static void Usage(string name)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Usage:");
        Console.WriteLine(usages[name]);
        Console.ResetColor();
        Environment.Exit(1);
    }

    private static void CommandParse(string com, string[] param)
    {
        if (!commands.ContainsKey(com)) Usage();
        commands[com](param);
    }

    private static void ShellLoop()
    {
        Console.Write(">");
        string[] s = Console.ReadLine().Split(' ');
        CommandParse(s[0], s[1..]);
        ShellLoop();
    }
}