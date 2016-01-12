using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Char
{
    public abstract class Char
    {
        public Guid Id;
        public Vector3 Position;
        public CharType charType;

        public int Movement;
        

        public Char(Vector2 position)
        {
            Id = Guid.NewGuid();
            Position = position;
        }

         
    }
}
