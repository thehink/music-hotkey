using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayMusic.Keybinder
{
    class KeybindEventArgs : EventArgs
    {
        public Keybind Keybind { get; set; }
    }
}
