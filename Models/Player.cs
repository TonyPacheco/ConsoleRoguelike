using System;
using System.Collections.Generic;
using System.Text;

namespace TurnBasedGame
{
    public class Player : Combatant
    {
        public int MaxHealth;
        public int Experience;
        public string ClassName;

        public Armour Head;
        public Armour Body;
        public Weapon OffHand;
        public Weapon DomHand;
        public Armour Legs;
        public Accessory Acc1;
        public Accessory Acc2;
        public Accessory Acc3;

        public static Player ConsoleCreate()
        {
            var p = new Player();
            p.Name = ConsoleRunner.Out("Welcome Adventurer, what's your name?", true);
            ConsoleRunner.UpdateName(p.Name);
            ConsoleRunner.UpdateLevel(0);
            return p;
        }
    }
}
