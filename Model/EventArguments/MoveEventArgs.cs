using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Model.EventArguments
{
    public class MoveEventArgs : EventArgs
    {
        public GridPoint NextPosition { get; private set; }

        public MoveEventArgs(GridPoint nextPosition)
        {
            NextPosition = nextPosition;
        }
    }
}
