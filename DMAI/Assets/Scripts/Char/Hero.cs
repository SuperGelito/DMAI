using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Char
{
    public abstract class Hero : Char
    {
        public HeroType heroType;
        public Hero(Vector2 position) : base(position)
        {
            this.charType = CharType.Hero;
        }
    }
}
