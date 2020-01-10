using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace cp365
{
    class VirtualFDD
    {
        private string executable;
        private string mountLetter;

        // конструктор
        public VirtualFDD()
        {
            this.executable = LocateExecutable();
            this.mountLetter = "A:";
        }

        private string LocateExecutable()
        {
            string sys32dir = Environment.SystemDirectory;
            if (!sys32dir.EndsWith("\\"))
                sys32dir += "\\";
            string imdiskFile = sys32dir + "imdisk.exe";
            if (File.Exists(imdiskFile))
                return imdiskFile;
            return null;
        }

        public bool Mount(string imageFileName, string letter="A:")
        {
            if (executable == null) return false;
            if (letter.Length < 2) return false;
            if (!letter.EndsWith(":"))
                letter += ":";
            Process ps = new Process();
            ps.StartInfo.FileName = executable;
            ps.StartInfo.Arguments = "-a -m "+letter+" -f " + imageFileName;
            ps.StartInfo.UseShellExecute = false;
            ps.Start();
            ps.WaitForExit();
            this.mountLetter = letter;
            ps.Dispose();
            return true;                
        }

        public bool Dismount()
        {
            if (executable == null) return false;
            Process ps = new Process();
            ps.StartInfo.FileName = executable;
            ps.StartInfo.Arguments = "-d -m " + this.mountLetter;
            ps.StartInfo.UseShellExecute = false;
            ps.Start();
            ps.WaitForExit();
            ps.Dispose();
            return true;
        }
    }
}
