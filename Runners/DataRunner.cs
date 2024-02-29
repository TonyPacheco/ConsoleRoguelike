namespace Roguelike.Runners
{
    public static class DataRunner
    {
        private const string WorldsDir = "./Data/Worlds/";
        private const string WorldsExt = ".wld";
        private const string PlayersDir = "./Data/Players/";
        private const string PlayersExt = ".plr";
        private const string SavesDir = "./Data/Saves/";
        private const string SavesExt = ".sav";

        public static World LoadWorldFromDisk(string fileName)
        {
            if(!fileName.EndsWith(WorldsExt))
            {
                fileName += WorldsExt;
            }
            var filePath = WorldsDir + fileName;
            using var reader = File.OpenText(filePath);
            var worldName = reader.ReadLine()!;
            var widthHeight = reader.ReadLine()!;

            var (width, height) = ParseNumbers(widthHeight);
            Coord? spawnPoint = null;

            var world = new World(worldName, width, height);

            for(var y = 0; y < height; ++y)
            {
                var line = reader.ReadLine()!;
                for(var x = 0; x < width; ++x)
                {
                    var character = line[x];
                    var coord = new Coord(x, y);
                    if(character == 'S')
                    {
                        spawnPoint = coord;
                    }
                    else
                    {
                        var tile = new Tile(character, coord);
                        world[x, y] = tile;
                    }
                }
            }
            if(!spawnPoint.HasValue)
            {
                throw new ArgumentException($"World loaded from {fileName} had no spawn point!");
            }
            world.SetSpawn(spawnPoint.Value);
            return world;
        }

        public static Player LoadPlayerFromDisk(string fileName)
        {
            if(!fileName.EndsWith(PlayersExt))
            {
                fileName += PlayersExt;
            }
            var filePath = PlayersDir + fileName;
            using var reader = File.OpenText(filePath);
            var name = reader.ReadLine()!;
            var clas = reader.ReadLine()!;
            var maxHealth = reader.ReadLine()!;
            return new Player()
            {
                Name = name,
                ClassName = clas,
                MaxHealth = int.Parse(maxHealth),
            };
        }

        public static void SaveGameStateToDisk(Game game, string fileName)
        {
            if(!fileName.EndsWith(SavesExt))
            {
                fileName += SavesExt;
            }
            var filePath = SavesDir + fileName;
            using var writer = File.CreateText(filePath);

            writer.WriteLine(game.Player.Name);
            writer.WriteLine(game.Player.ClassName);
            writer.WriteLine(game.Player.Experience);
            writer.WriteLine(game.Player.Health + " " + game.Player.MaxHealth);

            writer.WriteLine();

            writer.WriteLine(game.World.Name);
            writer.WriteLine(game.World.Width + " " + game.World.Height);
            writer.WriteLine(game.World.SpawnPoint!.Value.X + " " + game.World.SpawnPoint.Value.Y);
            writer.WriteLine(game.CurrentCoord.X + " " + game.CurrentCoord.Y);
            foreach(var tile in game.World.Tiles)
            {
                writer.WriteLine($"{tile.ToChar()}|{(tile.Discovered ? "D" : "O")}|{tile.TurnFirstEnterred}|{tile.BlurbOverride}");
            }

            writer.Close();
        }

        public static Game LoadGameStateFromDisk(string fileName)
        {
            if(!fileName.EndsWith(SavesExt))
            {
                fileName += SavesExt;
            }
            var filePath = SavesDir + fileName;
            using var reader = File.OpenText(filePath);

            var playerName = reader.ReadLine()!;
            var clas = reader.ReadLine()!;
            var experience = int.Parse(reader.ReadLine()!);
            var (health, maxHealth) = ParseNumbers(reader.ReadLine()!);
            reader.ReadLine();

            var player = new Player()
            {
                Name = playerName,
                ClassName = clas,
                Experience = experience,
                Health = health,
                MaxHealth = maxHealth
            };

            var worldName = reader.ReadLine()!;
            var (width, height) = ParseNumbers(reader.ReadLine()!);
            var (spawnX, spawnY) = ParseNumbers(reader.ReadLine()!);
            var spawnCoord = new Coord(spawnX, spawnY);
            var (currentX, currentY) = ParseNumbers(reader.ReadLine()!);
            var currentCoord = new Coord(currentX, currentY);

            var world = new World(worldName, width, height);
            for (var y = 0; y < height; y++)
            {
                for(var x = 0; x < width; x++)
                {
                    var coord = new Coord(x, y);
                    var tile = ParseTileState(reader.ReadLine()!, coord);
                    world[coord] = tile;
                }
            }
            world.SetSpawn(spawnCoord);
            return new Game(player, world, currentCoord);
        }

        /// <summary>
        /// Parses a string that has two numbers seperated by a space
        /// </summary>
        private static (int A, int B) ParseNumbers(string line)
        {
            var split = line.Split(' ');
            var a = int.Parse(split[0]);
            var b = int.Parse(split[1]);
            return (a, b);
        }

        private static Tile ParseTileState(string line, Coord coord)
        {
            var split = line.Split('|');
            var tileChar = split[0][0];
            var state = split[1];
            var turnEnterred = split[2];
            var blurbOverride = split[3];
            if(string.IsNullOrEmpty(blurbOverride))
            {
                //just in case it comes in as a space or empty string
                blurbOverride = null;
            }

            return new Tile(tileChar, coord, blurbOverride)
            {
                Discovered = state == "D",
                TurnFirstEnterred = int.Parse(turnEnterred)
            };
        }
    }
}
