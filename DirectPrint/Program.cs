using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectPrint
{
    class Program
    {
        static void Main(string[] args)
        {
            Print(GetPrintScreen());
        }

        public static Bitmap GetPrintScreen()
        {
            try
            {

                Bitmap print = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(print);
                graphics.CopyFromScreen(0, 0, 0, 0, print.Size);
                graphics.Dispose();

                return print;
            }
            catch
            {
                // ignored
            }

            return new Bitmap(1, 1);
        }

        public static void Print(Bitmap printScreen)
        {
            try
            {
                PrintDialog printDialog1 = new PrintDialog();
                printDialog1.Document = new PrintDocument
                { DefaultPageSettings = { Landscape = true, Margins = new Margins(20, 20, 20, 20) } };


                var defaultBounds = printDialog1.Document.DefaultPageSettings.Bounds;

                var maxSize = Math.Max(printScreen.Width, printScreen.Height);
                double mult = 4;

                if (defaultBounds.Width > defaultBounds.Height)
                    defaultBounds = new Rectangle(0, 0, (int)(maxSize * mult),
                        (int)(Math.Round((double)defaultBounds.Height / defaultBounds.Width * maxSize) * mult));

                if (defaultBounds.Width < defaultBounds.Height)
                    defaultBounds = new Rectangle(0, 0,
                        (int)(Math.Round((double)defaultBounds.Width / defaultBounds.Height * maxSize) * mult),
                        (int)(maxSize * mult));


                int psHeight = defaultBounds.Height;
                int psWidth = (int)Math.Round(((double)printScreen.Size.Width) / printScreen.Size.Height * psHeight);
                bool horizontalCentering = true;

                if (psWidth > defaultBounds.Width)
                {
                    psWidth = defaultBounds.Width;
                    psHeight = (int)Math.Round((double)printScreen.Size.Height / printScreen.Size.Width * psWidth);
                    horizontalCentering = false;
                }

                Bitmap adaptedBitmap = new Bitmap(defaultBounds.Width, defaultBounds.Height);
                Graphics graphics = Graphics.FromImage(adaptedBitmap);

                graphics.DrawImage(printScreen,
                    horizontalCentering
                        ? new Rectangle((defaultBounds.Width - psWidth) / 2, 0, psWidth, psHeight)
                        : new Rectangle(0, (defaultBounds.Height - psHeight) / 2, psWidth, psHeight));

                graphics.Dispose();
                printScreen.Dispose();

                printDialog1.Document.PrintPage += (s, e) => { e.Graphics.DrawImage(adaptedBitmap, e.MarginBounds); };

                
                DialogResult result = printDialog1.ShowDialog();

                if (result == DialogResult.OK)
                    printDialog1.Document.Print();

                printDialog1.Document.Dispose();
                adaptedBitmap.Dispose();
                printDialog1.Dispose();
            }
            catch
            {
                MessageBox.Show(@"Ошибка печати", @"Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
