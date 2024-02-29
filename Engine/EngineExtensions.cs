using ConsoleGameEngine;
using Roguelike.Engine.Extensions;

namespace Roguelike.Engine
{
    public static class EngineExtensions
    {
        private static int DefaultColor = Colors.White;

        public static void SetDefaultColor(this ConsoleEngine _, int color)
        {
            DefaultColor = color;
        }

        public static void WriteText(this ConsoleEngine engine, Point point, string text)
        {
            engine.WriteText(point, text, DefaultColor);
        }

        public static void WriteText(this ConsoleEngine engine, Point point, char character, int? color = null)
        {
            engine.WriteText(point, character.ToString(), color ?? DefaultColor);
        }
    }
}
