namespace Roguelike
{
    public static class GeneralExtensions
    {
        public static string Repeat(this char character, int count) 
        {
            return string.Concat(Enumerable.Repeat(character, count));
        }

        public static string Repeat(this string text, int count)
        {
            return string.Concat(Enumerable.Repeat(text, count));
        }
    }
}
