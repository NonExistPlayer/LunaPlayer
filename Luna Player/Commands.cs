using ConsoleTools.FileExplorer;

#pragma warning disable CS8604

namespace LunaPlayer;
partial class Program
{
    private static void CommandsInit()
    {
        commands.Add("play", (param) =>
        {
            string? path = null;
            float volume = 1;
            sbyte loop = 0;
            if (param.Length == 0) Usage("play");
            foreach (string p in param)
            {
                string arg = string.Join(":", p.Split(':')[1..]);
                switch (p.Split(':')[0])
                {
                    case "/path":
                        if (!File.Exists(arg)) Error($"File {arg} doesn't exist.");
                        path = arg;
                        break;
                    case "/volume":
                        if (!float.TryParse(arg, out _)) Error("/volume requires an numeric value within range 0,0 to 1,0");
                        volume = float.Parse(arg);
                        break;
                    case "/noui":
                        if (arg.Length != 0) Error("unexcepted use of arguments");
                        break;
                    case "/loop":
                        if (arg.Length == 0) {loop = -1; break; }
                        if (!sbyte.TryParse(arg, out _))
                            Error("/loop requires an numeric value within range 0 to 127");
                        loop = sbyte.Parse(arg);
                        if (loop < 0)
                            Error("/loop requires an numeric value within range 0 to 127");
                        break;
                    default:
                        Usage("play");
                        break;
                }
            }
            if (path == null) Usage("play");
            if (param.Contains("/noui"))
                Player.Play(path, float.Clamp(volume, 0, 1));
            else
                Player.PlayUI(path, float.Clamp(volume, 0, 1), loop);
        });
        commands.Add("about", (param) =>
        {
            if (param.Length != 0) Error("unexpected use of arguments.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(" _                        ______ _                       \r\n| |                       | ___ \\ |                      \r\n| |    _   _ _ __   __ _  | |_/ / | __ _ _   _  ___ _ __ \r\n| |   | | | | '_ \\ / _` | |  __/| |/ _` | | | |/ _ \\ '__|\r\n| |___| |_| | | | | (_| | | |   | | (_| | |_| |  __/ |   \r\n\\_____/\\__,_|_| |_|\\__,_| \\_|   |_|\\__,_|\\__, |\\___|_|   \r\n                                          __/ |          \r\n                                         |___/   ");
            Console.WriteLine(Version);
            Console.WriteLine("by NonExistPlayer / nonex / коробочка");
            Console.ResetColor();
        });
        commands.Add("shell", (param) =>
        {
            if (param.Length != 0) Error("unexpected use of arguments.");
            ShellLoop();
        });
        commands.Add("list-play", (param) =>
        {
            string path = "";
            foreach (string p in param)
            {
                string arg = string.Join(":", p.Split(':')[1..]);
                switch (p.Split(':')[0])
                {
                    case "/path":
                        if (!File.Exists(arg)) Error($"File {arg} doesn't exist.");
                        path = arg;
                        break;
                    default:
                        Usage("list-play");
                        break;
                }
            }
            Player.ListPlay(path);
        });
        commands.Add("explorer", (param) =>
        {
            string path = "\\";
            foreach (string p in param)
            {
                string arg = string.Join(":", p.Split(':')[1..]);
                switch (p.Split(':')[0])
                {
                    case "/path":
                        if (!Directory.Exists(arg)) Error($"Directory {arg} doesn't exist.");
                        path = arg;
                        break;
                    default:
                        Usage("explorer");
                        break;
                }
            }
            Console.Title = "Choose path to music file";
            Explorer exp = new(path, ".mp3|.wav");
            exp.FileSelected += () =>
            {
                Player.PlayUI(exp.SelectedFile);
            };
            exp.Run();
        });
    }
}