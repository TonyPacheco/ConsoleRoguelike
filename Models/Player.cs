namespace Roguelike
{
    public class Player : Combatant
    {
        public int MaxHealth = 100;
        public int Experience = 0; //for now, level up at increments of 100 xp
        public int Level => (Experience / 100) + 1;
        public string ClassName = "Adventurer";

        public Armour Head;
        public Armour Body;
        public Weapon OffHand;
        public Weapon DomHand;
        public Armour Legs;
        public Accessory Acc1;
        public Accessory Acc2;
        public Accessory Acc3;

        //public static Player ConsoleCreate()
        //{
        //    var p = new Player();
        //    p.Name = ConsoleRunner.Out("Welcome Adventurer, what's your name?", true);
        //    ConsoleRunner.UpdateName(p.Name);
        //    ConsoleRunner.UpdateLevel(0);
        //    return p;
        //}


        public Player()
        {
            Health = MaxHealth;
        }

        public static Player TestCreate()
        {
            return new Player
            {
                Name = "Tony"
            };
        }

        public int GetXpAsPercent()
        {
            return int.Parse(Experience.ToString("000")[1..]);
        }

        public int GetHealthAsPercent()
        {
            return Health / MaxHealth * 100;
        }
    }
}
