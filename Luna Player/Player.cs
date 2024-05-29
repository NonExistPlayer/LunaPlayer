using NAudio.Wave;
using System.Text.Json;
using TagLib;
using ConsoleTools.Frames;
using System.Text;
using System.Runtime.InteropServices;

#pragma warning disable CS8601
#pragma warning disable CS8629

namespace LunaPlayer;
public sealed class Player
{
    private static WaveOutEvent? waveOut;
    public static void Play(string filePath, float volume)
    {
        using var reader = new AudioFileReader(filePath);
        using var waveOut = new WaveOutEvent();
        waveOut.Init(reader);
        waveOut.Volume = volume;
        waveOut.Play();
        Console.CursorVisible = false;
        while (waveOut.PlaybackState != PlaybackState.Stopped)
        {
            Thread.Sleep(250);
        }
        Console.CursorVisible = true;
        Console.Clear();
    }
    private static TimeSpan getpos(IWavePosition wave, long ticks)
    {
        return TimeSpan.FromMilliseconds((ticks / (wave.OutputWaveFormat.Channels * wave.OutputWaveFormat.BitsPerSample / 8)) * 1000.0 / wave.OutputWaveFormat.SampleRate);
    }
    public static void PlayUI(string? filePath, float volume = 1, sbyte loop = 0)
    {
        waveOut = new();
        AudioFileReader? reader = null;
        try
        {
            reader = new(filePath);
        }
        catch (COMException)
        {
            Program.Error("specified mp3-file is corrupted.");
            return;
        }
        catch (EndOfStreamException)
        {
            Program.Error("specified wav-file is corrupted.");
            return;
        }
        catch (FormatException)
        {
            Program.Error("specified aiff-file is corrupted.");
            return;
        }
        using var waveChannel32 = new WaveChannel32(reader);
        using TagLib.File file = TagLib.File.Create(filePath);
        var stream = reader.ToSampleProvider();
        Tag tags = file.GetTag(TagTypes.Id3v2);
        Console.Title = $"{(string.IsNullOrWhiteSpace(tags.Title) ? Path.GetFileName(filePath) : tags.Title)}";
        waveOut.Init(reader);
        waveOut.Volume = volume;
        waveOut.Play();
        Task.Run(() =>
        {
            while (waveOut.PlaybackState != PlaybackState.Stopped)
            {
                ConsoleKeyInfo currentKey = Console.ReadKey(true);
                switch (currentKey.Key)
                {
                    case ConsoleKey.P:
                    case ConsoleKey.MediaPlay:
                    case ConsoleKey.Spacebar:
                        if (waveOut.PlaybackState == PlaybackState.Playing)
                            waveOut.Pause();
                        else
                            waveOut.Play();
                        break;
                    case ConsoleKey.RightArrow:
                        reader.Position += 1724130;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (reader.Position - 1724130 > 0)
                            reader.Position -= 1724130;
                        else 
                            reader.Position = 0;
                        break;
                    case ConsoleKey.DownArrow:
                        if (waveOut.Volume - 0.2f > 0)
                            waveOut.Volume -= 0.2f;
                        break;
                    case ConsoleKey.UpArrow:
                        if (waveOut.Volume + 0.2f <= 1)
                            waveOut.Volume += 0.2f;
                        break;
                    case ConsoleKey.D0:
                        reader.Position = 0;
                        break;
                    case ConsoleKey.MediaStop:
                    case ConsoleKey.S:
                    case ConsoleKey.Escape:
                        loop = 0;
                        waveOut.Stop(); 
                        break;
                }
            }
        });
        Console.CursorVisible = false;
        Console.Clear();
        Frame frame = [];
        frame.AddRange([
            !string.IsNullOrEmpty(tags.Title) && tags.Performers == null ? tags.Title :
            !string.IsNullOrWhiteSpace(tags.Title) && tags.Performers != null
            ? $"{string.Join(',', tags.Performers)} - {tags.Title}" :
            Path.GetFileNameWithoutExtension(filePath),
            "00:00 / 00:00 / -00:00",
            "[-----]"
        ]);
        frame.DrawTop();
        frame.Draw(0);
        frame.Draw(1);
        if (!string.IsNullOrEmpty(tags.Title) && tags.Performers != null)
        {
            Console.SetCursorPosition(4, 1);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(string.Join(',', tags.Performers));
            Console.SetCursorPosition(7 + string.Join(',', tags.Performers).Length, 1);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(tags.Title);
            Console.ResetColor();
            Console.Write("\n\n");
        }
        frame.Draw(2);
        frame.DrawLow();
        while (waveOut.PlaybackState != PlaybackState.Stopped)
        {
            Console.SetCursorPosition(4, 2);
            if (waveOut.PlaybackState == PlaybackState.Paused)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"{getpos(waveOut, reader.Position).ToString(@"mm\:ss")} / { reader.TotalTime.ToString(@"mm\:ss")} / -{ (reader.TotalTime - getpos(waveOut, reader.Position)).ToString(@"mm\:ss")}");
            Console.ResetColor();
            Console.SetCursorPosition(4, 3);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write($"[{new string('-', (int)(waveOut.Volume * 10)/2)}{new string('_', 5-(int)(waveOut.Volume * 10)/2)}]");
            Console.ResetColor();
        }
        Console.CursorVisible = true;
        Console.Clear();
        Console.Title = "lunaplayer";
        if (loop == 0) return;
        if (loop != -1)
            for (byte i = 0; i < loop - 1; i++)
            {
                PlayUI(filePath, waveOut.Volume);
            }
        else
            while (loop == -1)
            {
                PlayUI(filePath, waveOut.Volume);
            }
        waveOut.Dispose();
    }
    public static void ListPlay(string filePath)
    {
        bool exit = false;
        JsonDocument doc = JsonDocument.Parse(System.IO.File.ReadAllText(filePath));
        if (!doc.RootElement.TryGetProperty("list", out _))
            Program.Error("property 'list' is missing.");
        JsonElement[] music = [.. doc.RootElement.GetProperty("list").EnumerateArray()];
        for (int i = 0; i < music.Length; i++)
        {
            JsonElement el = music[i];
            Task.Run(() =>
            {
                while (!exit)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.MediaNext:
                            waveOut?.Stop();
                            i = int.Clamp(i + 1, 0, music.Length); 
                        break;
                        case ConsoleKey.MediaPrevious:
                            waveOut?.Stop();
                            i = int.Clamp(i - 1, 0, music.Length);
                        break;
                    }
                }
            });
            try
            {
                string? path = el.GetString();
                if (!System.IO.File.Exists(path))
                    Program.Error("specified file is missing.");
                PlayUI(path);
            } catch (Exception e)
            {
                Program.Error(e.ToString());
            }
        }
        exit = true;
    }
}