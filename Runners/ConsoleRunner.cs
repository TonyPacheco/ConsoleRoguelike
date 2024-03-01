using System.Diagnostics;
using ConsoleGameEngine;
using static Roguelike.World;
using Roguelike.Engine;
using Roguelike.Engine.Extensions;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Roguelike.Runners
{
    public static class ConsoleRunner
    {
        public static readonly char[] YN = ['y', 'n'];
        private static readonly int WIN_W = 120;
        private static readonly int WIN_H = 30;
        private static readonly Point FIRST_CHAR = new (0, 0);
        private static readonly Point FINAL_CHAR = new (WIN_W - 1, WIN_H - 1);
        private static readonly Point OUTPUT_PANE_START = new(SIDE_BAR_W + 1, TOP_BAR_H + 1);
        private static readonly Point OUTPUT_PANE_END = new(WIN_W - 1, WIN_H - 4);
        private static readonly Point CURSOR_INPUT = new (SIDE_BAR_W + 2, WIN_H - 2);
        private static readonly Point CURSOR_WRITE = new (SIDE_BAR_W + 4, TOP_BAR_H + 2);
        private static readonly Point HEADER_PANE_START = new (SIDE_BAR_W + 1, 1);
        private static readonly Point HEADER_PANE_END = new (WIN_W - 1, 3);
        private static readonly Point CURSOR_HEADER_TOP = new (SIDE_BAR_W + 4, TOP_BAR_H - 2);
        private static readonly Point CURSOR_HEADER_BTM = new (SIDE_BAR_W + 4, TOP_BAR_H - 1);
        private static readonly Point NAME_SLOT = new (2, 1);
        private static readonly Point XP_SLOT = new (2, 2);
        private static readonly Point HP_SLOT = new (15, 2);

        private static readonly Point MAP_CENTRE = new (SIDE_BAR_W / 2, 12);

        private const int TOP_BAR_H = 3;
        private const int SIDE_BAR_W = 30;
        private const char TOP_LEFT = '╔';
        private const char TOP_RIGHT = '╗';
        private const char BOT_LEFT = '╚';
        private const char BOT_RIGHT = '╝';
        private const char TOP_T = '╦';
        private const char BOT_T = '╩';
        private const char RIGHT_T = '╣';
        private const char LEFT_T = '╠';
        private const char CROSS = '╬';
        private const char SPACE = ' ';
        private const char BAR = '═';
        private const char PIPE = '║';

        private static readonly ConsoleEngine _engine = new(WIN_W, WIN_H, 8, 8);

        public static void Init()
        {
            _engine.Borderless();
            _engine.ClearBuffer();
            _engine.SetDefaultColor(Colors.White);
            _engine.Fill(FIRST_CHAR, FINAL_CHAR, Colors.Black);
            DrawRect(FIRST_CHAR, FINAL_CHAR);
            DrawBar(new (0, TOP_BAR_H), WIN_W, LEFT_T, RIGHT_T); //fix right connect
            DrawPipe(new (SIDE_BAR_W, 0), WIN_H, TOP_T, BOT_T);
            _engine.WriteText(new(SIDE_BAR_W, TOP_BAR_H), CROSS);
            DrawBar(new (SIDE_BAR_W, WIN_H - 3), WIN_W - SIDE_BAR_W, LEFT_T, RIGHT_T);
            DrawBar(new (0, SIDE_BAR_W + 6), SIDE_BAR_W + 1, LEFT_T, RIGHT_T); //fix right connect
            Refresh();
        }

        public static void Clear()
        {
            _engine.ClearBuffer();
        }

        public static void Refresh()
        {
            _engine.DisplayBuffer();
        }

        public static void WriteHeader(string topMsg, string btmMsg)
        {
            _engine.Fill(HEADER_PANE_START, HEADER_PANE_END, Colors.Black);
            _engine.WriteText(CURSOR_HEADER_TOP, topMsg);
            _engine.WriteText(CURSOR_HEADER_BTM, btmMsg);
            Refresh();
        }

        /// <summary>
        /// Writes a series of messages, with a pause awaiting a player key press between each if <paramref name="pauseBetweenMsgs"/> which defaults to true
        /// </summary>
        public static void Write(ICollection<string> msgs, bool pauseBetweenMsgs = true)
        {
            ClearOutputPane();
            for(var i = 0; i < msgs.Count; ++i)
            {
                var writePoint = new Point(CURSOR_WRITE.X, CURSOR_WRITE.Y + i);
                _engine.WriteText(writePoint, msgs.ElementAt(i));
                Refresh();
                if(pauseBetweenMsgs)
                {
                    Console.Read();
                }
            }
            if(!pauseBetweenMsgs)
            {
                // no pause between, but a final pause, otherwise the player would never see the output
                Console.Read();
            }
        }

        /// <summary>
        /// Writes a prompt and gets string input back from the player
        /// </summary>
        /// <returns>Choice with string value matching the input from the player</returns>
        public static Choice Prompt(string msg)
        {
            ClearOutputPane();
            _engine.WriteText(CURSOR_WRITE, msg);
            Refresh();
            var response = GetStringInput();
            return new Choice(response);
        }

        /// <summary>
        /// Writes a prompt and gets a choice of the options back from the player
        /// </summary>
        /// <returns>Choice with int value matching the input from the player, and string value matching the text of of the option selected</returns>
        public static Choice Prompt(string msg, OptionGroup options)
        {
            ClearOutputPane();
            _engine.WriteText(CURSOR_WRITE, msg);
            foreach(var (option, index) in options)
            {
                var writePoint = new Point(CURSOR_WRITE.X, CURSOR_WRITE.Y + index + 1);
                _engine.WriteText(writePoint, option.Display);
            }
            Refresh();

            while(true)
            {
                var choice = Console.ReadKey().KeyChar;
                if(options.TrySelectOption(choice, out var selected))
                {
                    return new Choice(selected);
                }
            }
        }

        public static void ClearOutputPane()
        {
            _engine.Fill(OUTPUT_PANE_START, OUTPUT_PANE_END, Colors.Black);
        }

        public static char GetCharInput(IEnumerable<char>? validOptions = null)
        {
            if(validOptions == null)
            {
                //anything is valid
                return Console.ReadKey().KeyChar;
            }

            while(true)
            {
                var choice = Console.ReadKey().KeyChar;
                if(validOptions.Contains(choice))
                {
                    return choice;
                }
            }
        }

        public static string GetStringInput()
        {
            var input = string.Empty;
            for(var press = Console.ReadKey(); press.Key != ConsoleKey.Enter; press = Console.ReadKey())
            {
                if(press.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input = input[..^1];
                }
                else
                {
                    input += press.KeyChar;
                }
                ResetInputField(input);
            }
            ResetInputField();
            return input;
        }

        public static void ResetInputField(string? newInput = null)
        {
            var blanks = ' '.Repeat(WIN_W - SIDE_BAR_W - 4);
            _engine.WriteText(CURSOR_INPUT, blanks);
            if(newInput != null)
            {
                _engine.WriteText(CURSOR_INPUT, newInput);
            }
            Refresh();
        }

        /// <summary>
        /// Updates player info with name and level; level is drawn using unicode number balls because I think they're neat
        /// </summary>
        public static void UpdatePlayerInfo(Player player)
        {
            var levelChar = Convert.ToChar(10101 + player.Level);
            _engine.WriteText(NAME_SLOT, $"{levelChar}  {player.Name}");
            var xpBlocksFull = player.GetXpAsPercent() / 10;
            _engine.WriteText(XP_SLOT, $"XP{'▰'.Repeat(xpBlocksFull)}{'▱'.Repeat(10 - xpBlocksFull)}");
            var healthBlocksFull = player.GetHealthAsPercent() / 10;
            _engine.WriteText(HP_SLOT, $"HP{'▰'.Repeat(healthBlocksFull)}{'▱'.Repeat(10 - healthBlocksFull)}");
            Refresh();
        }

        /// <summary>
        /// Draws the mini-map as a 9x9 grid around the players current position
        /// </summary>
        public static void UpdateMap(World world, Coord playerCoord)
        {
            for(var y = -4; y <= 4; ++y)
            {
                for(var x = -4; x <= 4; ++x)
                {
                    var drawPoint = new Point(MAP_CENTRE.X - x, MAP_CENTRE.Y + y);
                    var mapCoord = new Coord(playerCoord.X - x, playerCoord.Y + y);
                    if(world.CoordIsOffWorld(mapCoord) || !world[mapCoord].Discovered)
                    {
                        _engine.WriteText(drawPoint, SPACE);
                    }
                    else
                    {
                        _engine.WriteText(drawPoint, world[mapCoord].ToChar());
                    }
                }
            }
            _engine.WriteText(MAP_CENTRE, '☺');
            Refresh();
        }

        private static void DrawPipe(Point start, int height, char startCap = PIPE, char endCap = PIPE, int color = Colors.White)
        {
            _engine.WriteText(start, startCap, color);
            for(var i = 1; i < height - 1; ++i)
            {
                _engine.WriteText(new(start.X, start.Y + i), PIPE, color);
            }
            _engine.WriteText(new(start.X, start.Y + height), endCap, color);
        }

        private static void DrawBar(Point start, int width, char startCap = BAR, char endCap = BAR, int color = Colors.White)
        {
            _engine.WriteText(start, startCap, color);
            _engine.WriteText(new(start.X + 1, start.Y), BAR.Repeat(width - 2), color);
            _engine.WriteText(new(start.X + width, start.Y), endCap, color);
        }

        private static void DrawRect(Point start, Point end, int color = Colors.White)
        {
            DrawBar(start, end.X - start.X + 1, TOP_LEFT, TOP_RIGHT, color);
            DrawPipe(start, end.Y - start.Y + 1, TOP_LEFT, BOT_LEFT, color);
            DrawPipe(new (end.X, start.Y), end.Y - start.Y + 1, TOP_RIGHT, BOT_RIGHT, color);
            DrawBar(new (start.X, end.Y), end.X - start.X + 1, BOT_LEFT, BOT_RIGHT, color);
            _engine.WriteText(end, BOT_RIGHT, color); //this _shouldn't be needed, but is ...
        }

        [DebuggerDisplay("{x},{y}")]
        private struct CharCoord
        {
            public int x;
            public int y;

            public CharCoord(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
#if DEBUG
        public static class Debug
        {
            public static void DumpMap(World world)
            {
                var topLeftX = WIN_W - 2 - world.Width;
                var topLeftY = WIN_H - 3 - world.Height;
                for(var x = 0; x < world.Width; ++x)
                {
                    for(var y = 0; y < world.Height; ++y)
                    {
                        var drawPoint = new Point(topLeftX + x, topLeftY + y);
                        _engine.WriteText(drawPoint, world.Tiles[x, y].ToChar());
                    }
                }
                Refresh();
            }
        }
#endif
    }

    public class Choice
    {
        private int int_val;
        private string str_val;
        private Direction? dir_val => Enum.IsDefined(typeof(Direction), int_val) ? (Direction) int_val : null;

        public Choice(string s, int i = 0)
        {
            str_val = s;
            int_val = i;
        }

        public Choice(Option selected)
        {
            str_val = selected.Text;
            int_val = selected.Selector;
        }

        public static implicit operator char(Choice c) => (char) c.int_val;
        public static implicit operator int(Choice c) => c.int_val;
        public static implicit operator string(Choice c) => c.str_val;
        public static implicit operator Direction?(Choice c) => c.dir_val;
    }

    public class Option
    {
        public string Text { get; private set; }
        public char Selector { get; private set; }

        public string Display => $"[{Selector.ToString().ToUpperInvariant()}] {Text}";

        public Option(string text, char selector)
        {
            Selector = selector;
            Text = text;
        }

        public Option(string text, int selector)
        {
            Selector = selector.ToString()[0];
            Text = text;
        }

        public bool Selected(char input)
        {
            return Selector == input;
        }
    }

    public class OptionGroup : IEnumerable<(Option Option, int Index)>
    {
        public IEnumerable<(Option Option, int Index)> Options { get; private set; }
        public Dictionary<char, Option> OptionsKeyed { get; private set; }

        public OptionGroup(IEnumerable<Option> options)
        {
            Options = options.Select((o, i) => (o, i));
            OptionsKeyed = options.ToDictionary(o => o.Selector);
        }

        public OptionGroup(IEnumerable<string> options)
        {
            Options = options.Select((o, i) => (new Option(o, i), i));
            OptionsKeyed = Options.ToDictionary(o => o.Option.Selector, o => o.Option);
        }

        public bool TrySelectOption(char input, [NotNullWhen(true)] out Option? selected)
        {
            return OptionsKeyed.TryGetValue(input, out selected);
        }

        public IEnumerator<(Option Option, int Index)> GetEnumerator() => Options.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Options.GetEnumerator();
    }
}