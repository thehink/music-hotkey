using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlayMusic
{

    [Flags]
    public enum KeyState
    {
        Down,
        Up
    }

    public class KeypressEventArgs : EventArgs
    {
        public Keys Key { get; set; }

        public KeyState State { get; set; }
    }
}
