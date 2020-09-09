using System;
using System.Collections.Generic;
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

        private readonly List<Key> _hookedKeys = new List<Key>();
        private readonly Queue<Key> _lastKeys = new Queue<Key>();


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
                            _lastKeys.Enqueue(pressedKey);
                        

                        while (_lastKeys.Count > _hookedKeys.Count)
                            _lastKeys.Dequeue();

                        if (_hookedKeys.All(f => _lastKeys.Contains(f)))
                        {
                            _lastKeys.Clear();
                            ExternFunc?.Invoke();
                        }

                        foreach (var pressedKey in pressedKeys)
                            AllFunc?.Invoke(pressedKey);
                    }
                }
                Thread.Sleep(1);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public HookHelper(Key[] keys, FuncDelegate externFunc)
        {
            _hookedKeys.AddRange(keys);
            ExternFunc = externFunc;
            var th = new Thread(StartMonitor);
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }
    }

    public class DataSource
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private readonly HashSet<Key> _pressedKeys = new HashSet<Key>();

        public List<Key> GetNewPressedKeys()
        {
            List<Key> newPressedKeys = new List<Key>(10);

            foreach (Key key in Utils.GetEnumValues<Key>().Where(x => x != Key.None))
            {
                bool down = Keyboard.IsKeyDown(key);

                if (!down && _pressedKeys.Contains(key))
                    _pressedKeys.Remove(key);

                else if (down && !_pressedKeys.Contains(key)) // The key is pressed, but wasn't pressed before - it will be returned
                {
                    _pressedKeys.Add(key);
                    newPressedKeys.Add(key);
                }
            }

            return newPressedKeys;
        }
    }

    public static class Utils
    {
        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
