using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
namespace Assets.Scripts.Board
{
    public class Cell
    {
        public Cell()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id;
        public OverFloorType overFloor = OverFloorType.Floor;
        public Char.Char CellOwner = null;
        public bool IsFree { get { return CellOwner == null; } }
    }
}
