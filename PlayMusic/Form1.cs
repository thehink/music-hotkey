using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using PlayMusic.Keybinder;
using PlayMusic.Player;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace PlayMusic
{

    public partial class Form1 : Form, IDisposable
    {

        private KeybindManager keybindManager;
        private AudioPlaybackEngine player;
        private Settings settings;

        private List<string> Files { get; } = new List<string>();

        private string SelectedFile { get; set; }

        public List<string> Ids = new List<string>();

        private readonly Timer timer1 = new Timer();

        public Form1()
        {
            InitializeComponent();

            player = AudioPlaybackEngine.Instance;
            keybindManager = new KeybindManager();
            settings = Settings.LoadSettings();

            timer1.Interval = 400;
            timer1.Tick += timer1_Tick;

            textBox2.Text = settings.fileFolder;

            player.PlaybackEnded += OnPlaybackEnded;
            player.PlaybackStarted += OnPlaybackStarted;

            keybindManager.KeyPressed += KeyPressed;
            keybindManager.KeybindAdded += KeybindAdded;
            keybindManager.KeybindUpdated += KeybindUpdated;
            keybindManager.KeybindRemoved += KeybindRemoved;
            keybindManager.KeybindStopButton += KeybindStopButton;

            keybindManager.RefreshKeybinds();

            PopulateFiles();
            RefreshDevices();
            PopulateKeys();
        }

        private void OnPlaybackEnded(object sender, PlaybackEventArgs e)
        {
            //Console.WriteLine("ended");
            KeyboardEvent.SendKey(Keys.Home, KeyState.Up);
        }

        private void OnPlaybackStarted(object sender, PlaybackEventArgs e)
        {
            //Console.WriteLine("started");
            KeyboardEvent.SendKey(Keys.Home, KeyState.Down);
        }

        private void KeybindStopButton(object sender, KeybindEventArgs e)
        {
            player.StopSounds();
        }

        private void KeybindUpdated(object sender, KeybindEventArgs e)
        {
            for(var i = 0; i < dataGridView1.Rows.Count; ++i)
            {
                string id = (string)dataGridView1.Rows[i].Cells[0].Value;
                if(id == e.Keybind.Id)
                {
                    dataGridView1.Rows[i].Cells[1].Value = e.Keybind.Filename;
                }
            }
            
        }

        private void KeyPressed(object sender, KeybindEventArgs e)
        {
            player.PlayCachedSound(e.Keybind.Filename);
        }

        private void KeybindAdded(object sender, KeybindEventArgs e)
        {
            Ids.Add(e.Keybind.Id);
            dataGridView1.Rows.Add(e.Keybind.Id, e.Keybind.Filename, e.Keybind.Key);
        }

        private void KeybindRemoved(object sender, KeybindEventArgs e)
        {
            
        }

        private void PopulateKeys()
        {
            comboBox2.Items.Clear();

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                comboBox2.Items.Add(key.ToString());
            }

            comboBox2.SelectedIndex = 0;
        }

        private void RefreshDevices()
        {
            comboBox1.Items.Clear();

            for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
            {
                var capabilities = WaveOut.GetCapabilities(deviceId);
                comboBox1.Items.Add(capabilities.ProductName);
            }

            comboBox1.SelectedIndex = 0;
        }

        private void PopulateFiles()
        {
            Files.Clear();

            if (Directory.Exists(settings.fileFolder))
            {
                string[] fileEntries = Directory.GetFiles(settings.fileFolder);
                foreach (string fileName in fileEntries)
                {
                    Files.Add(Path.GetFileName(fileName));
                }

            }

            FilterList();
        }

        private void FilterList()
        {
            listBox1.Items.Clear();

            listBox1.Items.AddRange(Files.FindAll(e => e.ToLower().Contains(textBox1.Text.ToLower())).ToArray());

            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
        }

        private void SelectAudio(string file)
        {
            SelectedFile = file;
            fileNameLabel.Text = file;
        }



        protected override void WndProc(ref Message m)
        {

            
            base.WndProc(ref m);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PopulateFiles();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            player.PlayCachedSound(SelectedFile);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RefreshDevices();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            FilterList();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectAudio(settings.fileFolder + "\\" + listBox1.SelectedItem.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Media Files|*.mp3;*.mp4;*.wav";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SelectAudio(openFileDialog1.FileName);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var key = (Keys) Enum.Parse(typeof(Keys), comboBox2.Text);
            keybindManager.AddKeybind(key, SelectedFile);
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            string id = Ids[e.RowIndex];
            Ids.RemoveAt(e.RowIndex);
            keybindManager.RemoveKeybind(id);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            player.ChangeDeviceId(comboBox1.SelectedIndex);
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            var id = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            keybindManager.UpdateKeybind(id, SelectedFile);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.InitialDirectory = settings.fileFolder;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    textBox2.Text = dialog.FileName;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            if(textBox2.Text != settings.fileFolder)
            {
                string path = textBox2.Text;

                if (path.EndsWith("\\") || path.EndsWith("/"))
                {
                    path = path.TrimEnd(new char[]{ '\\', '/'});
                }

                settings.fileFolder = path;
                PopulateFiles();
                settings.Save();
                Console.WriteLine(path);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            timer1.Stop();
            timer1.Start();
        }
    }
}
