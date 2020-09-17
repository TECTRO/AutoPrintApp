using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForegroundShowApp
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static IntPtr GetPrintHWnd(int n)
        {
            if (n > 0)
            {
                Thread.Sleep(200);
                IntPtr printWnd = FindWindow(null, "Печать");

                return 
                    printWnd.ToString() == "0" ? 
                    GetPrintHWnd(n - 1) : 
                    printWnd;
            }

            return FindWindow(null, "Печать");
        }

        public static void Main()
        {
            SetForegroundWindow(GetPrintHWnd(30));
        }
    }
}
