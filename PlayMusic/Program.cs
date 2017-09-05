using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlayMusic
{
    static class Program
    {

        private static IntPtr _hookID = IntPtr.Zero;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InterceptKeys.Hook();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            InterceptKeys.UnHook();
        }
    }
}
