using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace PrintApp
{
    public partial class Form1 : Form
    {
        private readonly HookHelper _hook;

        public Form1()
        {
            InitializeComponent();

            _hook = new HookHelper(
                new[]
                {
                    Key.LeftShift,
                    Key.LeftCtrl,
                    Key.LeftAlt
                },
                Execute);

            SynchronizationContext context = SynchronizationContext.Current;

            _hook.AllFunc += e =>
            {
                context.Send(e1 =>
                {
                    listBox1.Items.Add(e);
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                },null);
                
            };
        }

        private void Execute()
        {
            Process.Start("DirectPrint.exe");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _hook.StopMonitor();
        }

        //windowUpdater===============================================================================

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
          
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            IntPtr printWnd = FindWindow(null, "Печать");

            if (printWnd.ToString() != "0")
                SetForegroundWindow(printWnd);
        }

        //===========================================================================================

    }
}
