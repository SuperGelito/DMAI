using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Char
{
    public class Fighter: Hero
    {
        public Fighter(Vector2 position) : base(position)
        {
            this.heroType = HeroType.Fighter;
        }
    }
}
