using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Model
{   
    /// <summary>
    /// A park bejárata
    /// </summary>
    public class Gate : Building
    {
        private static Gate? _instance;

        /// <summary>
        /// A kapu egyetlen példánya
        /// </summary>
        public static Gate Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new Gate();
                return _instance;
            }
            private set { _instance = value; }
        }

        private Gate() : base("Főbejárat",new GridPoint(20,9),3,1,2, new System.Drawing.Point(1,0)) { Status = FacilityStatus.Working; }

        
        public override void TryUsing(Visitor visitor)
        {
            if (this is Gate)
            {
                if (VisitorLeaving != null)
                {
                    VisitorLeaving(this, visitor);
                }
            }
        }

        /// <summary>
        /// Akkor hívódik meg, ha egy látogató belép a kapun.
        /// </summary>
        public event EventHandler<Visitor>? VisitorLeaving;
    }
}
