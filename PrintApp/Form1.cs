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
    }
}
