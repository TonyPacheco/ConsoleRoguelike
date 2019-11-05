using System;
using System.Collections.Generic;
using System.Text;
using static TurnBasedGame.ConsoleRunner;

namespace TurnBasedGame
{
    public class World
    {
        private const string WORLDS_END = "The ends of the world.";
        private static readonly Dictionary<char, string> TRAVEL_OPTIONS;

        private readonly Tile[,] _tiles;
        private readonly int _width, _height;
        private readonly Coord _spawnPoint;

        public enum Direction
        {
            North = 'w',
            West = 'a',
            South = 's',
            East = 'd',
        }

        static World()
        {
            TRAVEL_OPTIONS = new Dictionary<char, string>();
        }

        public World(int width, int height, Coord spawnPoint)
        {
            _spawnPoint = spawnPoint;
            _width = width;
            _height = height;
            _tiles = new Tile[width, height];
        }

        public static World BuildTestWorld()
        {
            var random = new Random();
            var world = new World(5, 5, new Coord(2, 2));
            for(var i = 0; i < 5; ++i)
            {
                for(var j = 0; j < 5; ++j)
                {
                    var tileType = random.Next(1, 7);
                    world[i, j] = new Tile((TileType) tileType);
                }
            }
            world[world._spawnPoint] = new Tile(TileType.Spawn);
            return world;
        }

        public Tile this[int i, int j]
        {
            get => _tiles[i, j];
            set => _tiles[i, j] = value;
        }

        public Tile this[Coord coord]
        {
            get => _tiles[coord.x, coord.y];
            set => _tiles[coord.x, coord.y] = value;
        }

        public void Init()
        {
            EnterTile(_spawnPoint);
            _tiles[_spawnPoint.x, _spawnPoint.y] = new Tile(TileType.Town);
        }

        private Tile CurrentTile => _tiles[Game.Instance.CurrentCoord.x, Game.Instance.CurrentCoord.y];

        private Coord? ToNorth(Coord baseCoord)
        {
            if(baseCoord.y == 0)
                return null;

            baseCoord.y -= 1;
            return baseCoord;
        }

        private Coord? ToEast(Coord baseCoord)
        {
            if(baseCoord.x == _width - 1)
                return null;

            baseCoord.x += 1;
            return baseCoord;
        }

        private Coord? ToSouth(Coord baseCoord)
        {
            if(baseCoord.y == _height - 1)
                return null;

            baseCoord.y += 1;
            return baseCoord;
        }

        private Coord? ToWest(Coord baseCoord)
        {
            if(baseCoord.x == 0)
                return null;

            baseCoord.x -= 1;
            return baseCoord;
        }

        private Tile GetNeighbor(Coord baseCoord, Direction direction)
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
            return neighborCoord == null ? null : _tiles[neighborCoord.Value.x, neighborCoord.Value.y];
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

        private void EnterTile(Coord coord)
        {
            EnterTile(coord.x, coord.y);
        }

        private void EnterTile(int x, int y)
        {
            Game.Instance.CurrentCoord = new Coord(x, y);
            var tile = _tiles[x, y];
            ClearOutputField();
            DiscoverNeighbors(Game.Instance.CurrentCoord);
            UpdateMap();
            Out(tile.Blurb);
        }

        private void DiscoverNeighbors(Coord baseCoord)
        {
            var n = GetNeighborCoord(baseCoord, Direction.North);
            if(n.HasValue)
                _tiles[n.Value.x, n.Value.y].Discovered = true;
            n = GetNeighborCoord(baseCoord, Direction.East);
            if(n.HasValue)
                _tiles[n.Value.x, n.Value.y].Discovered = true;
            n = GetNeighborCoord(baseCoord, Direction.South);
            if(n.HasValue)
                _tiles[n.Value.x, n.Value.y].Discovered = true;
            n = GetNeighborCoord(baseCoord, Direction.West);
            if(n.HasValue)
                _tiles[n.Value.x, n.Value.y].Discovered = true;
        }

        public bool OffTheWorld(Coord coord)
        {
            return coord.x < 0 || coord.x >= _width || coord.y < 0 || coord.y >= _height;
        }

        public void EnterNeighbor(Direction direction)
        {
            var n = GetNeighborCoord(Game.Instance.CurrentCoord, direction);
            if(!n.HasValue)
            {
                ClearOutputField();
                Out(new string[] { "Oh no, you fell off the world and died!",
                                   "Better luck next time."});
                Environment.Exit(69);
            }
            EnterTile(n.Value);
        }

        public Direction TravelPrompt()
        {
            var current = Game.Instance.CurrentCoord;
            TRAVEL_OPTIONS['w'] = $"To the north: {GetNeighbor(current, Direction.North)?.Type.ToString() ?? WORLDS_END}";
            TRAVEL_OPTIONS['a'] = $"To the west: {GetNeighbor(current, Direction.West)?.Type.ToString() ?? WORLDS_END}";
            TRAVEL_OPTIONS['s'] = $"To the south: {GetNeighbor(current, Direction.South)?.Type.ToString() ?? WORLDS_END}";
            TRAVEL_OPTIONS['d'] = $"To the east: {GetNeighbor(current, Direction.East)?.Type.ToString() ?? WORLDS_END}";
            return (Direction) Out("Travel?", TRAVEL_OPTIONS);
        }
    }

    public enum TileType
    {
        Spawn,
        City,
        Town,
        Forest, 
        Plains,
        Camp,
        Hills,
        Farm,
        Coast,
        Ocean
    }

    public class Tile
    {
        private static readonly string[] _blurbs = new string[] {
            "You find yourself in small town, maybe you should explore a bit?",
            "A bustling city, stone walls and tall towers with squat row houses at their bases.",
            "A quaint town with a single main street and a few off shooting alleys.",
            "Tall green trees and short green bushes, it's awfully foresty around.",
            "Cart paths and migration trails cross the green and gold plains crossed and crissed.",
            "A single wisp of smoke rises slowly from beside a worn canvas lean-to.",
            "Softly undulatintg peaks and valleys for acres.",
            "The fields here are squared and fenced, it's a farm alright.",
            "The bronze waves crash on the sandy shores, you can see for miles over the horizon.",
            "It's water as far as your eyes can sea. Ocean right?",
        };

        private static readonly char[] _mapTiles = new char[]
        {
            'ѧ',
            'Ѩ',
            'ѧ',
            '♣',
            '░',
            'ϫ',
            '\u0488',
            '▓',
            '▞',
            '█',
        };

        public string Blurb;

        public TileType Type { get; set; }

        public bool Discovered = false;

        public char ToChar()
        {
            return _mapTiles[(int) Type];
        }

        public Tile(TileType type, string blurb = null)
        {
            Type = type;
            if(blurb == null)
            {
                Blurb = _blurbs[(int)type];
            }
        }
    }

    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
