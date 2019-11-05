using System;
using System.Collections.Generic;
using System.Text;

namespace TurnBasedGame
{

    public enum Stat
    {
        Speed,
        Strength,
        Reflexes, 
        Dexterity,
        Skirmishing
    }

    public class Combatant
    {
        public string Name;
        public int Health;

        public int Speed;
        public int Strength;
        public int Reflexes;
        public int Dexterity;
        public int Skirmishing;
    }
}
