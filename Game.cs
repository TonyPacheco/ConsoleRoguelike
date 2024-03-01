using Roguelike.Runners;

namespace Roguelike
{
    public class Game
    {
        public static Game Instance { get; private set;  } = null!;
        public Player Player { get; private set; }
        public World World { get; private set; }
        public Coord CurrentCoord { get; set; }
        public long CurrentTurnNumber { get; private set; } = 1;

        public Game(Player player, World world)
        {
            Instance = this;
            Player = player;
            World = world;
            CurrentCoord = World.SpawnPoint!.Value;
            ConsoleRunner.UpdatePlayerInfo(player);
        }

        public Game(Player player, World world, Coord currentCoord)
        {
            Instance = this;
            Player = player;
            World = world;
            CurrentCoord = currentCoord;
            ConsoleRunner.UpdatePlayerInfo(player);
        }

        public void Start()
        {
            World.Init();
            ConsoleRunner.UpdateMap(World, CurrentCoord);
            UpdateCurrentLocationOutput();
            MainLoop();
        }
        
        public void MainLoop()
        {
            var quit = false;
            for(CurrentTurnNumber = 1L; !quit; CurrentTurnNumber++)
            {
                //Move
                var travelDirection = GetTravelDirection();
                var newTile = World.EnterNeighbor(travelDirection);
                
                //Draw
                ConsoleRunner.UpdateMap(World, CurrentCoord);

                //Output
                var messages = new List<string>
                {
                    newTile.Blurb
                };
                if(newTile.TurnFirstEnterred == CurrentTurnNumber)
                {
                    Player.Experience += 1;
                    messages.Add("You gain 1xp for exploring a new area.");
                }
                UpdateCurrentLocationOutput();
                ConsoleRunner.Write(messages, false);

                //Update
                ConsoleRunner.UpdatePlayerInfo(Player);
            }
        }

        public void UpdateCurrentLocationOutput()
        {
            var currentTile = World[CurrentCoord];
            ConsoleRunner.WriteHeader("Current Location:", $"{currentTile.Type} @ [{currentTile.Coord.X},{currentTile.Coord.Y}]");
        }

        /// <summary>
        /// Outputs options for travel direction and awaits player input
        /// </summary>
        /// <returns>Players's selected direction of travel</returns>
        public World.Direction GetTravelDirection()
        {
            var options = new OptionGroup(new Option[]
            {
                GetTravelOptionForDirection(World.Direction.North),
                GetTravelOptionForDirection(World.Direction.East),
                GetTravelOptionForDirection(World.Direction.South),
                GetTravelOptionForDirection(World.Direction.West),
            });
            var choice = ConsoleRunner.Prompt("Travel?", options);
            return InputCharToDirection(choice);
        }

        private Option GetTravelOptionForDirection(World.Direction direction)
        {
            var current = World[CurrentCoord];
            return new(current.GetTravelPrompt(direction), DirectionToInputChar(direction));
        }

        private static char DirectionToInputChar(World.Direction direction) => direction switch
        {
            World.Direction.North => 'w',
            World.Direction.West => 'a',
            World.Direction.South => 's',
            World.Direction.East => 'd',
            _ => throw new NotImplementedException(),
        };

        private static World.Direction InputCharToDirection(char input) => input switch
        {
            'w' => World.Direction.North,
            'a' => World.Direction.West,
            's' => World.Direction.South,
            'd' => World.Direction.East,
            _ => throw new NotImplementedException()
        };
    }
}