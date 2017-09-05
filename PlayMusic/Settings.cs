using Newtonsoft.Json;
using PlayMusic.Keybinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayMusic
{
    class Settings
    {

        private static string filename = @"settings.json";

        public List<Keybind> Keybinds = new List<Keybind>();

        public static Settings settings;

        public static Settings LoadSettings()
        {

            if(Settings.settings != null)
            {
                return settings;
            }

            if (!File.Exists(filename))
            {
                return new Settings();
            }

            using (StreamReader r = new StreamReader(Settings.filename))
            {
                string json = r.ReadToEnd();
                Settings settings = JsonConvert.DeserializeObject<Settings> (json);
                Settings.settings = settings;
                return settings;
            }
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this);

            using (StreamWriter file = File.CreateText(Settings.filename))
            {
                file.Write(json);
            }
        }
    }
}
