namespace TurnBasedGame
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleRunner.Init();
            new Game(Player.ConsoleCreate(), World.BuildTestWorld())
                .Start();
        }
    }
}
