using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace PrintApp
{
    public class HookHelper
    {
        private readonly object _syncPlug = new object();
        public enum HowToRegister
        {
            KeyUp,
            KeyDown,
            None
        }

        private readonly List<Key> HookedKeys = new List<Key>();
        private readonly Queue<Key> LastKeys = new Queue<Key>();


        public delegate void FuncDelegate();

        public FuncDelegate ExternFunc;

        public delegate void AllFuncDelegate(Key r);

        public AllFuncDelegate AllFunc;

        public void StartMonitor()
        {
            DataSource dataSource = new DataSource();

            while (true)
            {
                // Get pressed keys and saves them
                List<Key> pressedKeys = dataSource.GetNewPressedKeys();
                if (pressedKeys.Any())
                {
                    lock (_syncPlug)
                    {
                        foreach (var pressedKey in pressedKeys)
                            LastKeys.Enqueue(pressedKey);
                        

                        while (LastKeys.Count > HookedKeys.Count)
                            LastKeys.Dequeue();

                        if (HookedKeys.All(f => LastKeys.Contains(f)))
                        {
                            LastKeys.Clear();
                            ExternFunc?.Invoke();
                        }

                        foreach (var pressedKey in pressedKeys)
                            AllFunc?.Invoke(pressedKey);
                    }
                }
                Thread.Sleep(1);
            }
        }

        public HookHelper(Key[] keys, FuncDelegate externFunc, HowToRegister howTo)
        {
            HookedKeys.AddRange(keys);
            ExternFunc = externFunc;
            var th = new Thread(StartMonitor);
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }
    }

    public class DataSource
    {
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private HashSet<Key> PressedKeys = new HashSet<Key>();




        /// <summary>
        /// This functions scans currently pressed keys and returns them. Every key is returned just once. If the key is still pressed during second
        /// method call, it is not returned. It's returned again after the key is released and pressed again.
        /// </summary>
        /// <returns>List of keys which were just pressed</returns>
        public List<Key> GetNewPressedKeys()
        {
            List<Key> newPressedKeys = new List<Key>(10);

            // Get state of every key we know
            foreach (Key key in Utils.GetEnumValues<Key>().Where(x => x != Key.None))
            {
                // Is it pressed?
                bool down = Keyboard.IsKeyDown(key);

                // It's not pressed, but it was - we consider this key as released
                if (!down && PressedKeys.Contains(key))
                    PressedKeys.Remove(key);
                else if (down && !PressedKeys.Contains(key)) // The key is pressed, but wasn't pressed before - it will be returned
                {
                    PressedKeys.Add(key);
                    newPressedKeys.Add(key);
                }
            }

            return newPressedKeys;
        }

        /// <summary>
        /// Creates snapshot of computer screen and returns its image
        /// </summary>
        /// <returns>Image of the screen</returns>
        public Bitmap GetScreenSnapshot()
        {
            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            return bitmap;
        }

        /// <summary>
        /// Search for currently active window (focused) and returns name of the process of that window.
        /// So if user is using Chrome right now, 'chrome' string will be returned.
        /// </summary>
        /// <returns>Name of the process who is tied to currently active window</returns>
        public string GetActiveWindowProcessName()
        {
            IntPtr windowHandle = GetForegroundWindow();
            GetWindowThreadProcessId(windowHandle, out uint processId);
            Process process = Process.GetProcessById((int)processId);

            return process.ProcessName;
        }
    }

    public static class Utils
    {
        /// <summary>
        /// Gets all values of given enum
        /// </summary>
        /// <typeparam name="T">Enum whose values we want</typeparam>
        /// <returns>Values of the enum</returns>
        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
