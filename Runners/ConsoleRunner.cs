using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using static TurnBasedGame.World;
using static System.Console;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TurnBasedGame
{
    public static class ConsoleRunner
    {
        public static readonly char[] YN = new char[] { 'y', 'n' };
        private static ConsoleColor currentColor = ConsoleColor.White;
        private static readonly int WIN_W = LargestWindowWidth;
        private static readonly int WIN_H = LargestWindowHeight;
        private static readonly CharCoord FIRST_CHAR = new CharCoord(0, 0);
        private static readonly CharCoord FINAL_CHAR = new CharCoord(WIN_W - 1, WIN_H - 1);
        private static readonly CharCoord CURSOR_HOME = new CharCoord(SIDE_BAR_W + 2, WIN_H - 2);
        private static readonly CharCoord CURSOR_WRITE = new CharCoord(SIDE_BAR_W + 4, TOP_BAR_H + 4);
        private static readonly CharCoord CURSOR_HEADER = new CharCoord(SIDE_BAR_W + 4, TOP_BAR_H + 2);
        private static readonly CharCoord NAME_SLOT = new CharCoord(2, 1);
        private static readonly CharCoord LEVEL_SLOT = new CharCoord(2, 2);
        private static readonly CharCoord MAP_CENTRE = new CharCoord(SIDE_BAR_W/2, SIDE_BAR_W + 12);

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

        #region Externals For Console Window Control
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static readonly IntPtr _thisConsole = GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion

        public static void Init()
        {
            OutputEncoding = Encoding.UTF8;
            ShowWindow(_thisConsole, 3);
            Clear();
            SetBufferSize(WIN_W, WIN_H);
            SetWindowSize(WIN_W, WIN_H);
            DrawRect(FIRST_CHAR, FINAL_CHAR);
            DrawBar(new CharCoord(0, TOP_BAR_H), WIN_W, LEFT_T, RIGHT_T); 
            DrawPipe(new CharCoord(SIDE_BAR_W, 0), WIN_H, TOP_T, BOT_T);  
            WriteAt(CROSS, SIDE_BAR_W, TOP_BAR_H);                        
            DrawBar(new CharCoord(SIDE_BAR_W, WIN_H - 3), WIN_W - SIDE_BAR_W, LEFT_T, RIGHT_T);
            DrawBar(new CharCoord(0, SIDE_BAR_W + 6), SIDE_BAR_W + 1, LEFT_T, RIGHT_T);
            CursorCoords = CURSOR_WRITE;
        }

        public static void Out(string[] msgs)
        {
            CursorCoords = CURSOR_WRITE;
            for(var i = 0; i < msgs.Length; ++i)
            {
                Write(msgs[i]);
                ClearLineToBorder();
                SetCursorPosition(CURSOR_WRITE.x, CURSOR_WRITE.y + i + 1);
                ReadLine();
                SetCursorPosition(CURSOR_WRITE.x, CURSOR_WRITE.y + i + 1);
            }
        }

        public static Choice Out(string msg, bool requiresInput = false)
        {
            CursorCoords = CURSOR_WRITE;
            Write(msg);
            ClearLineToBorder();
            if(!requiresInput)
            {
                SetCursorPosition(CURSOR_WRITE.x, CURSOR_WRITE.y + 1);
                var garbage = ReadLine();
                CursorCoords = CURSOR_HOME;
                return new Choice(garbage);
            }
            CursorCoords = CURSOR_HOME;
            var response = ReadLine();
            while(string.IsNullOrEmpty(response))
            {
                CursorCoords = CURSOR_HOME;
                response = ReadLine();
            }
            ResetInputField();
            return new Choice(response);
        }

        public static Choice Out(string msg, string[] options)
        {
            CursorCoords = CURSOR_WRITE;
            Write(msg);
            LoopChar(SPACE, WIN_W - SIDE_BAR_W - msg.Length - 5);
            for(var i = 0; i < options.Length; ++i)
            {
                SetCursorPosition(CURSOR_WRITE.x, CURSOR_WRITE.y + i + 1);
                Write($"[{i + 1}] {options[i]}");
                ClearLineToBorder();
            }
            CursorCoords = CURSOR_HOME;
            var choice = 0;
            while(choice == 0)
            {
                CursorCoords = CURSOR_HOME;
                if(int.TryParse(ReadKey().KeyChar.ToString(), out choice))
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

        public static Choice Out(string msg, char[] options)
        {
            CursorCoords = CURSOR_WRITE;
            Write(msg);
            ClearLineToBorder();
            for(var i = 0; i < options.Length; ++i)
            {
                Write($"{options[i]}");
                ClearLineToBorder();
            }
            CursorCoords = CURSOR_HOME;
            char choice = ReadKey().KeyChar;
            while(!options.Contains(choice))
            {
                CursorCoords = CURSOR_HOME;
                choice = ReadKey().KeyChar;
            }
            ResetInputField();
            return new Choice(choice.ToString());
        }

        public static Choice Out(string msg, Dictionary<char, string> options)
        {
            CursorCoords = CURSOR_WRITE;
            Write(msg);
            ClearLineToBorder();
            var i = 1;
            foreach(var option in options)
            {
                SetCursorPosition(CURSOR_WRITE.x, CURSOR_WRITE.y + i++);
                Write($"[{option.Key.ToString().ToUpper()[0]}] {option.Value}");
                ClearLineToBorder();
            }
            CursorCoords = CURSOR_HOME;
            var choice = 'x';
            while(choice == 'x')
            {
                CursorCoords = CURSOR_HOME;
                choice = ReadKey().KeyChar;
            
                if(options.Keys.Contains(choice))
                {
                    ResetInputField();
                    return new Choice(options[choice], choice);
                }
                choice = 'x';
            }
            throw new Exception("WTF?");
        }

        public static void SetTextColor(ConsoleColor color)
        {
            currentColor = color;
            ForegroundColor = color;
        }

        public static void ResetInputField()
        {
            CursorCoords = CURSOR_HOME;
            LoopChar(SPACE, WIN_W - SIDE_BAR_W - 3);
            CursorCoords = CURSOR_HOME;
        }

        public static void UpdateName(string name)
        {
            WriteAt(name, NAME_SLOT);
            CursorCoords = CURSOR_HOME;
        }

        public static void UpdateLevel(int level)
        {
            WriteAt($"Lvl: {level}", LEVEL_SLOT);
            CursorCoords = CURSOR_HOME;
        }

        public static void UpdateMap()
        {
            for(var y = -4; y <= 4; ++y)
            {
                CursorCoords = new CharCoord(MAP_CENTRE.x - 4, MAP_CENTRE.y + y);
                for(var x = -4; x <= 4; ++x)
                {
                    var mapCoord = new Coord(Game.Instance.CurrentCoord.x + x, Game.Instance.CurrentCoord.y + y);
                    if(Game.Instance.World.OffTheWorld(mapCoord) || !Game.Instance.World[mapCoord].Discovered)
                    {
                        Write(SPACE);
                    }
                    else
                    {
                        Write(Game.Instance.World[mapCoord].ToChar());
                    }
                }
            }
            WriteAt('☺', MAP_CENTRE);
        }

        public static void ClearOutputField(bool preserveHeader = false)
        {
            FillRect(SPACE, preserveHeader ? CURSOR_HOME : CURSOR_HEADER, new CharCoord(WIN_W - 1, WIN_H - 3));
        }

        private static void WriteAt(char c, int x, int y)
        {
            SetCursorPosition(x, y);
            Write(c);
        }

        private static void WriteAt(string s, int x, int y)
        {
            SetCursorPosition(x, y);
            Write(s);
        }

        private static void WriteAt(char c, CharCoord cc)
        {
            SetCursorPosition(cc.x, cc.y);
            Write(c);
        }

        private static void WriteAt(string s, CharCoord cc)
        {
            SetCursorPosition(cc.x, cc.y);
            Write(s);
        }

        private static void DrawPipe(CharCoord start, int height, char startCap = PIPE, char endCap = PIPE, ConsoleColor color = ConsoleColor.White)
        {
            ForegroundColor = color;
            WriteAt(startCap, start.x, start.y);
            SetCursorPosition(start.x, start.y + 1);
            LoopChar(PIPE, height - 2, true);
            WriteAt(endCap, start.x, start.y + height - 1);
            ForegroundColor = currentColor;
        }

        private static void DrawBar(CharCoord start, int width, char startCap = BAR, char endCap = BAR, ConsoleColor color = ConsoleColor.White)
        {
            ForegroundColor = color;
            WriteAt(startCap, start.x, start.y);
            LoopChar(BAR, width - 2);
            Write(endCap);
            ForegroundColor = currentColor;
        }

        private static void DrawRect(CharCoord start, CharCoord end, ConsoleColor color = ConsoleColor.White)
        {
            DrawBar(start, end.x - start.x + 1, TOP_LEFT, TOP_RIGHT, color);
            DrawPipe(start, end.y - start.y + 1, TOP_LEFT, BOT_LEFT, color);
            DrawPipe(new CharCoord(end.x, start.y), end.y - start.y + 1, TOP_RIGHT, BOT_RIGHT, color);
            DrawBar(new CharCoord(start.x, end.y), end.x - start.x + 1, BOT_LEFT, BOT_RIGHT, color);
        }

        private static void FillRect(char c, CharCoord start, CharCoord end, ConsoleColor color = ConsoleColor.White)
        {
            CursorCoords = start;
            ForegroundColor = color;
            var width = end.x - start.x;
            var height = end.y - start.y;
            for(int i = 1; i <= height; ++i)
            {
                LoopChar(c, width);
                SetCursorPosition(start.x, start.y + i);
            }
            ForegroundColor = currentColor;
        }

        private static void ClearLineToBorder()
        {
            for(int i = CursorLeft; i < WIN_W - 1; ++i)
            {
                Write(SPACE);
            }
        }

        private static void LoopChar(char c, int n, bool vertical = false)
        {
            if(vertical)
            {
                var init_x = CursorCoords.x;
                for(var i = 0; i < n; ++i)
                {
                    Write(c);
                    SetCursorPosition(init_x, CursorTop + 1);
                }
            }
            else for(var i = 0; i < n; ++i) Write(c);
        }

        private static CharCoord CursorCoords
        {
            get => new CharCoord(CursorLeft, CursorTop);
            set => SetCursorPosition(value.x, value.y);
        }

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
    }
    public class Choice
    {
        private int int_val;
        private string str_val;
        private Direction? dir_val => Enum.IsDefined(typeof(Direction), int_val) ? (Direction) int_val : (Direction?) null;

        public Choice(string s, int i = 0)
        {
            str_val = s;
            int_val = i;
        }

        public static implicit operator int (Choice c) => c.int_val;
        public static implicit operator string (Choice c) => c.str_val;
        public static implicit operator Direction? (Choice c) => c.dir_val;
    }
}