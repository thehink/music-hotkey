using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlayMusic.Keybinder
{
    class KeybindManager
    {
        public event KeybindEventHandler KeyPressed;
        public event KeybindEventHandler KeybindAdded;
        public event KeybindEventHandler KeybindUpdated;
        public event KeybindEventHandler KeybindRemoved;
        public event KeybindEventHandler KeybindStopButton;
        public delegate void KeybindEventHandler(object sender, KeybindEventArgs e);

        private Settings settings = Settings.LoadSettings();

        public List<Keybind> Keybinds;

        public KeybindManager()
        {
            InterceptKeys.KeyEvent += KeyEvent;
            Keybinds = settings.Keybinds;
        }

        public void RefreshKeybinds()
        {
            Keybinds.ForEach(keybind =>
            {
                KeybindAdded(this, new KeybindEventArgs() { Keybind = keybind });
            });
        }

        public void UpdateKeybind(string id, string filename)
        {
            var keybind = Keybinds.First(k => k.Id == id);

            keybind.Filename = filename;

            KeybindUpdated(this, new KeybindEventArgs() { Keybind = keybind });

            settings.Save();
        }

        public void AddKeybind(Keys key, string filename)
        {
            var keybind = new Keybind()
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Filename = filename
            };

            Keybinds.Add(keybind);

            KeybindAdded(this, new KeybindEventArgs() { Keybind = keybind });

            settings.Save();
        }

        public void RemoveKeybind(string id)
        {
            var keybind = Keybinds.First(k => k.Id == id);

            Keybinds.Remove(keybind);

            KeybindRemoved(this, new KeybindEventArgs() { Keybind = keybind });

            settings.Save();
        }

        public void KeyEvent(KeypressEventArgs e)
        {
            Keybinds.ForEach(keybind =>
            {
                if(keybind.Key == e.Key && e.State == KeyState.Down)
                {
                    KeyPressed(this, new KeybindEventArgs() { Keybind = keybind });
                }
            });

            if(e.Key == Keys.NumPad0 && e.State == KeyState.Up)
            {
                KeybindStopButton(this, new KeybindEventArgs() { });
            }
            
        }
    }
}
