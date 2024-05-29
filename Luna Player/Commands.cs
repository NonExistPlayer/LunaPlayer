using ConsoleTools;

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
            return;

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
            foreach (string p in param)
            {
                string arg = string.Join(":", p.Split(':')[1..]);
                switch (p.Split(':')[0])
                {
                    default:
                        Usage("explorer");
                        break;
                }

            }
            Console.WriteLine("Select the path to the music.");
            string? path = Terminal.ShowOpenFileDialog("Supported Audio Formats (*.wav, *.mp3, *.aiff)\0*.wav;*.mp3;*.aiff\0" +
                "MP3 Audio (*.mp3)\0*.mp3\0" +
                "WAVE Audio (*.wav)\0*.wav\0" +
                "Audio Interchange File Format (*.aiff)\0*.aiff\0");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(new string(' ', "Select the path to the music.".Length));
            if (path == null) return;
            Player.PlayUI(path);
        });
    }
}