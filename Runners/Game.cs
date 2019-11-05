using System;
using static TurnBasedGame.ConsoleRunner;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TurnBasedGame
{
    public class Game
    {
        public const string GAME_NAME = "GAME NAME";

        public static Game Instance
        {
            get; private set;
        }

        public Player Player
        {
            get; private set;
        }

        public World World
        {
            get; private set;
        }

        public Coord CurrentCoord
        {
            get; set;
        }

        public Game(Player player, World world)
        {
            if(Instance == null)
                Instance = this;
            else
                throw new Exception("Cannot instantiate more that one Game object.");

            Player = player;
            World = world;
        }

        public void Start()
        {
            World.Init();
            MainLoop();
        }

        public void MainLoop()
        {
            while(true)
            {
                var travelDirection = World.TravelPrompt();
                World.EnterNeighbor(travelDirection);
            }
        }
    }
}
