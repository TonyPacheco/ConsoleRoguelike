using static Roguelike.World;

namespace Roguelike
{
    public enum TileType
    {
        OffWorld = -1,

        Spawn = 0,

        City = 1,
        Town = 2,
        Forest = 3,
        Plains = 4,
        Camp = 5,
        Hills = 6,
        Farm = 7,

        Coast = 8,
        Ocean = 9
    }

    public class Tile
    {
        public static readonly Tile OffWorld = new(TileType.OffWorld, new(-1, -1), "Oh no, you fell off the world and died!");

        private static readonly Dictionary<TileType, string> s_blurbs = new()
        {
            { TileType.OffWorld, "Oh no, you fell off the world and died!" },
            { TileType.Spawn, "You find yourself in small town, maybe you should explore a bit?" },
            { TileType.City, "A bustling city, stone walls and tall towers with squat row houses at their bases." },
            { TileType.Town, "A quaint town with a single main street and a few off shooting alleys." },
            { TileType.Forest, "Tall green trees and short green bushes, it's awfully foresty around." },
            { TileType.Plains, "Cart paths and migration trails cross the green and gold plains." },
            { TileType.Camp, "A single wisp of smoke rises slowly from beside a worn canvas lean-to." },
            { TileType.Hills, "Softly undulatintg peaks and valleys for acres." },
            { TileType.Farm, "The fields here are squared and fenced, it's a farm alright." },
            { TileType.Coast, "The bronze waves crash on the sandy shores, you can see for miles over the horizon." },
            { TileType.Ocean, "It's water as far as your eyes can sea. Ocean right?" },
        };

        private static readonly char[] s_mapTiles =
        [
            ' ',      //OffWorld (not actually used)
            'S',      //Spawn
            'Ѩ',      //City
            'ѧ',      //Town
            '♣',      //Forest
            '▓',      //Plains
            'Ϫ',      //Camp
            '∩',      //Hills
            'Ж',      //Farm
            '▞',      //Coast
            '█',      //Ocean
        ];

        public Coord Coord { get; private set; }
        public string Blurb => BlurbOverride ?? s_blurbs[Type];
        public string? BlurbOverride = null;
        public TileType Type { get; set; }

        /// <summary>
        /// Tile has been seen from a neighboring tile, not necessarily entered
        /// </summary>
        public bool Discovered = false;
        public long TurnFirstEnterred = -1;
        public bool Enterred => TurnFirstEnterred != -1;

        public Tile?[] Neighbors { get; private set; } = new Tile[4];

        private Tile() { }

        public Tile(char character, Coord coord, string? blurbOverride = null)
        {
            Coord = coord;
            Type = TileTypeFromChar(character);
            if(Type == TileType.Spawn)
            {
                throw new ArgumentException($"Use {nameof(CreateSpawn)} to create a Spawn tile, not the constructor");
            }
            BlurbOverride = blurbOverride;
        }

        public Tile(TileType type, Coord coord, string? blurbOverride = null)
        {
            Coord = coord;
            Type = type;
            if(Type == TileType.Spawn)
            {
                throw new ArgumentException($"Use {nameof(CreateSpawn)} to create a Spawn tile, not the constructor");
            }
            BlurbOverride = blurbOverride;
        }

        public static Tile GenerateRandom(Coord coord, Random? random = null)
        {
            random ??= new Random();
            var tileType = (TileType) random.Next(1, 8);
            return new Tile(tileType, coord);
        }

        public char ToChar() => s_mapTiles[((int) Type) + 1];
        
        public override string ToString() => ToChar().ToString();
        
        public Tile? GetNeighbor(Direction direction) => Neighbors[(int) direction];
        
        public string GetTravelPrompt(Direction direction) => $"To the {direction.ToString().ToLowerInvariant()}: {GetNeighbor(direction)?.Type.ToString() ?? ("The ends of the world.")}";

        public static TileType TileTypeFromChar(char character)
        {
            for(var i = 2; i < s_mapTiles.Length; ++i)
            {
                if(s_mapTiles[i] == character)
                {
                    return (TileType) i - 1;
                }
            }
            throw new NotImplementedException($"Character {character} cannot be mapped to a TileType");
        }

        public static Tile CreateSpawn(Coord coord)
        {
            return new Tile()
            {
                Coord = coord,
                Type = TileType.Spawn,
                Discovered = true,
                TurnFirstEnterred = 1,
            };
        }
    }
}
