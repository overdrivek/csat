#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;

namespace CSatEng
{
    public static class Log
    {
        private static System.IO.StreamWriter logWriter = null;

        public static void Create(string filename)
        {
            if (logWriter == null) logWriter = new System.IO.StreamWriter(filename);
        }
        public static void Close()
        {
            if (logWriter == null) return;
            logWriter.Close();
            logWriter = null;
        }
        public static void WriteToFile(string str)
        {
            if (logWriter == null)
            {
                Create("log.txt");
            }

            logWriter.WriteLine("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] " + str);
            logWriter.Flush();
        }

        public static void WriteLine(string str)
        {
            WriteLine(str, true);
        }
        public static void WriteLine(string str, bool writeToLog)
        {

#if DEBUG   // jos DEBUG, kirjoita myös konsoliin
            System.Diagnostics.Trace.WriteLine(str);
            Console.WriteLine(str);
#endif
            if (writeToLog) WriteToFile(str);
        }

        public static void Error(string str)
        {
            Log.WriteLine(str);
#if !DEBUG
            System.Windows.Forms.MessageBox.Show(str, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
#endif
            throw new Exception(str);
        }

    }

}
