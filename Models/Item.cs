using System;
using System.Collections.Generic;
using System.Text;

namespace TurnBasedGame
{

    public enum ArmourEquipType
    {
        Head,
        Body,
        Feet,
    }

    public enum WeaponEquipType
    {
        OffHand,
        DomHand,
        BothHands,
    }

    public class Item
    {
        public string Name;
        public string Blurb;
        public int Value;
    }

    public abstract class Augmentor
    {
        public List<Augmentation> Augmentations;
        public abstract bool Equip();
    }

    public class Weapon : Augmentor
    {
        public WeaponEquipType EquipType;
        public Stat HitChanceStat;
        public Stat DamageStat;

        public override bool Equip()
        {
            throw new NotImplementedException();
        }
    }

    public class Armour : Augmentor
    {
        public ArmourEquipType EquipType;

        public override bool Equip()
        {
            throw new NotImplementedException();
        }
    }

    public class Accessory : Augmentor
    {
        public override bool Equip()
        {
            throw new NotImplementedException();
        }
    }



}
