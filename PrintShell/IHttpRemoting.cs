using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.IO;

namespace PrintShell
{
    public interface IHttpRemoting
    {
        string SendPressedKey(string key);
    }

    public class MyHttpRemoting : MarshalByRefObject, IHttpRemoting
    {
        public string SendPressedKey(string key)
        {
            string path = @"c:\KeyTable.txt";
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(key);
            writer.Close();
            return key;
        }
    }
}