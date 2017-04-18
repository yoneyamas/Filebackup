using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Filebackup {
    class Log {
        static public void Write(string msg) {
            StreamWriter writer = new StreamWriter("log.txt");
            writer.WriteLine(msg);
            writer.Close();          
        }
    }
}
