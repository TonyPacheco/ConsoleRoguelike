using System.Diagnostics;
using ConsoleGameEngine;
using static Roguelike.World;
using Roguelike.Engine;
using Roguelike.Engine.Extensions;

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
        private static readonly Point CURSOR_HEADER = new (SIDE_BAR_W + 4, TOP_BAR_H + 1);
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

        public static void Out(ICollection<string> msgs)
        {
            ClearOutputPane();
            for(var i = 0; i < msgs.Count; ++i)
            {
                var writePoint = new Point(CURSOR_WRITE.X, CURSOR_WRITE.Y + i);
                _engine.WriteText(writePoint, msgs.ElementAt(i));
                Refresh();
                Console.Read();
            }
        }

        public static void ClearOutputPane()
        {
            _engine.Fill(OUTPUT_PANE_START, OUTPUT_PANE_END, Colors.Black);
        }

        public static Choice Out(string msg, bool requiresInput = false)
        {
            ClearOutputPane();
            _engine.WriteText(CURSOR_WRITE, msg);
            Refresh();
            if(!requiresInput)
            {
                return new Choice(string.Empty);
            }

            var response = GetInput();
            return new Choice(response);
        }

        public static Choice Out(string msg, string[] options)
        {
            ClearOutputPane();
            _engine.WriteText(CURSOR_WRITE, msg);
            for(var i = 0; i < options.Length; ++i)
            {
                var writePoint = new Point(CURSOR_WRITE.X, CURSOR_WRITE.Y + i + 1);
                _engine.WriteText(writePoint, $"[{i + 1}] {options[i]}");
            }
            Refresh();
            var choice = 0;
            while(choice == 0)
            {
                if(int.TryParse(Console.ReadKey().KeyChar.ToString(), out choice))
                {
                    if(choice != 0 && choice <= options.Length)
                    {
                        ResetInputField();
                        return new Choice(options[choice - 1], choice);
                    }
                    choice = 0;
                }
            }
            throw new Exception("WTF?");
        }

        //public static Choice Out(string msg, char[] options)
        //{
        //    CursorCoords = CURSOR_WRITE;
        //    Write(msg);
        //    ClearLineToBorder();
        //    for(var i = 0; i < options.Length; ++i)
        //    {
        //        Write($"{options[i]}");
        //        ClearLineToBorder();
        //    }
        //    CursorCoords = CURSOR_HOME;
        //    char choice = ReadKey().KeyChar;
        //    while(!options.Contains(choice))
        //    {
        //        CursorCoords = CURSOR_HOME;
        //        choice = ReadKey().KeyChar;
        //    }
        //    ResetInputField();
        //    return new Choice(choice.ToString());
        //}

        public static Choice Out(string msg, Dictionary<char, string> options)
        {
            ClearOutputPane();
            _engine.WriteText(CURSOR_WRITE, msg);
            var i = 1;
            foreach(var option in options)
            {
                var text = $"[{option.Key.ToString().ToUpperInvariant()}] {option.Value}";
                _engine.WriteText(new(CURSOR_WRITE.X, CURSOR_WRITE.Y + i), text);
                i++;
            }
            Refresh();

            var choice = '+';
            while(choice == '+')
            {
                ResetInputField();
                choice = Console.ReadKey().KeyChar;

                if(options.Keys.Contains(choice))
                {
                    ResetInputField();
                    return new Choice(options[choice], choice);
                }
                choice = '+';
            }
            throw new Exception("WTF?");
        }

        public static string GetInput()
        {
            var input = string.Empty;
            var press = Console.ReadKey();
            while(press.Key != ConsoleKey.Enter)
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
                press = Console.ReadKey();
            }
            ResetInputField();
            return input;
        }

        public static void SetCursorForInput()
        {
            Console.SetCursorPosition(CURSOR_INPUT.X, CURSOR_INPUT.Y);
        }

        public static void ResetInputField()
        {
            var blanks = ' '.Repeat(WIN_W - SIDE_BAR_W - 4);
            _engine.WriteText(CURSOR_INPUT, blanks);
            Refresh();
        }

        public static void ResetInputField(string newInput)
        {
            var blanks = ' '.Repeat(WIN_W - SIDE_BAR_W - 4);
            _engine.WriteText(CURSOR_INPUT, blanks);
            _engine.WriteText(CURSOR_INPUT, newInput);
            Refresh();
        }

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

        public static void ClearOutputField(bool preserveHeader = false)
        {
            //FillRect(SPACE, preserveHeader ? CURSOR_HOME : CURSOR_HEADER, new CharCoord(WIN_W - 1, WIN_H - 3));
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

        //private static void ClearLineToBorder()
        //{
        //    for(int i = CursorLeft; i < WIN_W - 1; ++i)
        //    {
        //        Write(SPACE);
        //    }
        //}

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

        public static implicit operator char(Choice c) => c.str_val[0];
        public static implicit operator int(Choice c) => c.int_val;
        public static implicit operator string(Choice c) => c.str_val;
        public static implicit operator Direction?(Choice c) => c.dir_val;
    }
}