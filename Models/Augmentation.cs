using System;
using System.Collections.Generic;
using System.Text;

namespace TurnBasedGame
{
    public class Augmentation
    {
        public Stat Affectee;
        public int Effect;
        public bool IsBuff => Effect > 0;
    }
}
