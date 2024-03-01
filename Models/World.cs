using System.Diagnostics;
using static Roguelike.World;

namespace Roguelike
{
    public class World
    {
        private static readonly Dictionary<char, string> TravelOptions = new();
        public string Name { get; private set; }
        public Tile[,] Tiles { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Coord? SpawnPoint { get; private set; }

        public enum Direction
        {
            North = 0,
            East = 1,
            South = 2,
            West = 3,
        }

        public World(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
        }

        /// <summary>
        /// Builds a random 5x5 World with a central spawn location
        /// </summary>
        public static World BuildTestWorld()
        {
            var random = new Random();
            var world = new World("TEST", 5, 5);
            for(var x = 0; x < 5; ++x)
            {
                for(var y = 0; y < 5; ++y)
                {
                    var coord = new Coord(y, x);
                    world[coord] = Tile.GenerateRandom(coord, random);
                }
            }
            world.SetSpawn(new Coord(2, 2));
            return world;
        }

        public Tile this[int i, int j]
        {
            get => Tiles[i, j];
            set => Tiles[i, j] = value;
        }

        public Tile this[Coord coord]
        {
            get => Tiles[coord.X, coord.Y];
            set => Tiles[coord.X, coord.Y] = value;
        }

        public void SetSpawn(Coord spawnCoord)
        {
            SpawnPoint = spawnCoord;
            var spawnTile = Tile.CreateSpawn(spawnCoord);
            spawnTile.Type = TileType.Town;
            this[spawnCoord] = spawnTile;
        }

        public void Init()
        {
            if(SpawnPoint == null)
            {
                throw new NotImplementedException("SpawnPoint must be set with SetSpawn() before initalizing World");
            }
            EnterTile(Game.Instance.CurrentCoord);
        }

        private Coord? ToNorth(Coord baseCoord)
        {
            if(baseCoord.Y == 0)
                return null;

            baseCoord.Y -= 1;
            return baseCoord;
        }

        private Coord? ToEast(Coord baseCoord)
        {
            if(baseCoord.X == Width - 1)
                return null;

            baseCoord.X += 1;
            return baseCoord;
        }

        private Coord? ToSouth(Coord baseCoord)
        {
            if(baseCoord.Y == Height - 1)
                return null;

            baseCoord.Y += 1;
            return baseCoord;
        }

        private Coord? ToWest(Coord baseCoord)
        {
            if(baseCoord.X == 0)
                return null;

            baseCoord.X -= 1;
            return baseCoord;
        }

        private Tile? GetNeighbor(Coord baseCoord, Direction direction)
        {
            Coord? neighborCoord = null;
            switch(direction)
            {
                case Direction.North:
                    neighborCoord = ToNorth(baseCoord);
                    break;
                case Direction.East:
                    neighborCoord = ToEast(baseCoord);
                    break;
                case Direction.South:
                    neighborCoord = ToSouth(baseCoord);
                    break;
                case Direction.West:
                    neighborCoord = ToWest(baseCoord);
                    break;
            }
            if(neighborCoord == null)
            {
                return null;
            }
            else
            {
                return Tiles[neighborCoord.Value.X, neighborCoord.Value.Y];
            }
        }

        private Coord? GetNeighborCoord(Coord baseCoord, Direction direction)
        {
            Coord? neighborCoord = null;
            switch(direction)
            {
                case Direction.North:
                    neighborCoord = ToNorth(baseCoord);
                    break;
                case Direction.East:
                    neighborCoord = ToEast(baseCoord);
                    break;
                case Direction.South:
                    neighborCoord = ToSouth(baseCoord);
                    break;
                case Direction.West:
                    neighborCoord = ToWest(baseCoord);
                    break;
            }
            return neighborCoord;
        }

        private Tile EnterTile(Coord coord)
        {
            Game.Instance.CurrentCoord = coord;
            var tile = this[coord];
            if(tile.TurnFirstEnterred == -1)
            {
                tile.TurnFirstEnterred = Game.Instance.CurrentTurnNumber;
            }
            DiscoverNeighbors(tile, coord);
            return tile;
        }

        private void DiscoverNeighbors(Tile tile, Coord baseCoord)
        {
            DiscoverNeighbor(tile, GetNeighborCoord(baseCoord, Direction.North), Direction.North);
            DiscoverNeighbor(tile, GetNeighborCoord(baseCoord, Direction.East), Direction.East);
            DiscoverNeighbor(tile, GetNeighborCoord(baseCoord, Direction.South), Direction.South);
            DiscoverNeighbor(tile, GetNeighborCoord(baseCoord, Direction.West), Direction.West);
        }

        private void DiscoverNeighbor(Tile baseTile, Coord? neighborCoord, Direction directionFromBase)
        {
            if(neighborCoord is null) //there is no neighboring tile in this direction
                return;

            var neighborTile = Tiles[neighborCoord.Value.X, neighborCoord.Value.Y];
            neighborTile.Discovered = true;
            baseTile.Neighbors[(int) directionFromBase] = neighborTile;
        }

        public bool CoordIsOffWorld(Coord coord)
        {
            return coord.X < 0 || coord.X >= Width || coord.Y < 0 || coord.Y >= Height;
        }

        public Tile EnterNeighbor(Direction direction)
        {
            var n = GetNeighborCoord(Game.Instance.CurrentCoord, direction);
            return n.HasValue 
                ? EnterTile(n.Value) 
                : Tile.OffWorld;
        }
    }

    [DebuggerDisplay("{X},{Y}")]
    public struct Coord(int x, int y)
    {
        public int X = x;
        public int Y = y;

        public readonly Coord NewCoordFromMove(Direction direction)
        {
            return direction switch
            {
                Direction.North => new(X, Y + 1),
                Direction.West => new(X - 1, Y),
                Direction.South => new(X, Y - 1),
                Direction.East => new(X + 1, Y),
                _ => throw new NotImplementedException(),
            };
        }

        public static bool operator == (Coord a, Coord b) => a.X == b.X && a.Y == b.Y;
        public static bool operator != (Coord a, Coord b) => a.X != b.X || a.Y != b.Y;
        public override bool Equals(object? obj) => obj is not null and Coord other && this == other;
        public override int GetHashCode() => (X * 10000) + Y;
    }
}
