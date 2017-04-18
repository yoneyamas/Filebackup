using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Filebackup {
    class BackupFile {
        
        public List<BackupDirectory> backupDir;
        const string backupDirListFile = "C:\\Programming\\Filebackup\\Filebackup\\.backup";
        public BackupFile() {
            backupDir = new List<BackupDirectory>();
        }

        public async Task addBackupDirectory(string fullPath, ObservableCollection<Node> nodes) {
            if(!Directory.Exists(fullPath)) {
                throw new ArgumentException(fullPath + " is not a directory");
            }
            foreach(var dir in backupDir) {
                if(fullPath == dir.OriginalDirPath) {
                    return;
                }
            }
            backupDir.Add(new BackupDirectory(fullPath));
            nodes.Add(new Node(fullPath));

            
            await allSyncBackupDir();
         

        }

        public void loadBackupDir(string filePath = backupDirListFile) {
            if(!File.Exists(filePath)) {
                throw new ArgumentException("Not found backup directory list file");
            }
            
            StreamReader reader = new StreamReader(filePath);

            while (!reader.EndOfStream) {
                string dir = reader.ReadLine();
                if(!Directory.Exists(dir)) {
                    Log.Write("not found backup directory \"" + dir + "\"");
                    continue;
                }
                backupDir.Add(new BackupDirectory(dir));
            }

            reader.Close();

        }

        public void writeBackupDir(string filePath = backupDirListFile) {
            if (!File.Exists(filePath)) {
                throw new ArgumentException("Not found backup directory list file");
            }

            StreamWriter writer = new StreamWriter(filePath,false);

            foreach(var dir in backupDir) {
                writer.WriteLine(dir.OriginalDirPath);
            }
            writer.Close();

        }
        //バックアップリストからすべてのファイルをチェックし，バックアップを行う．
        public async Task allSyncBackupDir() {
            foreach(var dir in backupDir) {
                await dir.updateBackupDir();
                dir.deleteDiffBackupFiles();
            }
        }

    }


}
