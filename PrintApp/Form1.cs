using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                    Keys.S, 
                    Keys.LControlKey, 
                    Keys.LShiftKey
                },
                Execute, 
                HookHelper.HowToRegister.KeyUp);
        }
        
        private void Execute()
        {
            Print(GetPrintScreen());
        }

        public Bitmap GetPrintScreen()
        {
            Bitmap print = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(print);
            graphics.CopyFromScreen(0, 0, 0, 0, print.Size);
            graphics.Dispose();
            return print;
        }

        public void Print(Bitmap printScreen)
        {
            printDialog1.Document = new PrintDocument {DefaultPageSettings = {Landscape = true, Margins = new Margins(20, 20, 20, 20)} };

            
            var defaultBounds = printDialog1.Document.DefaultPageSettings.Bounds;

            var maxSize = Math.Max(printScreen.Width, printScreen.Height);
            double mult = 4;

            if (defaultBounds.Width > defaultBounds.Height)
                defaultBounds = new Rectangle(0, 0, (int)(maxSize* mult), (int)(Math.Round((double)defaultBounds.Height / defaultBounds.Width * maxSize)* mult));
            
            if (defaultBounds.Width < defaultBounds.Height)
                defaultBounds = new Rectangle(0, 0, (int)(Math.Round((double)defaultBounds.Width / defaultBounds.Height *maxSize)* mult), (int)(maxSize* mult));
            

            int psHeight = defaultBounds.Height;
            int psWidth = (int)Math.Round(((double)printScreen.Size.Width) / printScreen.Size.Height * psHeight);
            bool horizontalCentering = true; 

            if (psWidth > defaultBounds.Width)
            {
                psWidth = defaultBounds.Width;
                psHeight = (int)Math.Round((double)printScreen.Size.Height / printScreen.Size.Width * psWidth);
                horizontalCentering = false;
            }

            Bitmap adaptedBitmap = new Bitmap(defaultBounds.Width,defaultBounds.Height);
            Graphics graphics = Graphics.FromImage(adaptedBitmap);

            graphics.DrawImage(printScreen,
                horizontalCentering
                    ? new Rectangle((defaultBounds.Width - psWidth) / 2, 0, psWidth, psHeight)
                    : new Rectangle(0, (defaultBounds.Height - psHeight) / 2, psWidth, psHeight));

            graphics.Dispose();


            printDialog1.Document.PrintPage += (s, e) =>
            {
                e.Graphics.DrawImage(adaptedBitmap, e.MarginBounds);
            };

            
            DialogResult result = printDialog1.ShowDialog();
            
            if (result == DialogResult.OK)
                printDialog1.Document.Print();
        }
    }
}
