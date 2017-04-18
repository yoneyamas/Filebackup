using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Filebackup {
    class BackupDirectory {
        private string originalDirPath;
        public string OriginalDirPath {
            get {
                return originalDirPath;
            }
        }
        private string backupDirPath;
        public string BackupDirPath {
            get { return backupDirPath; }
            private set { backupDirPath = value; }
        }
        private FileSystemWatcher watcher;
        static public string rootBackupDirectoryPath = "F:\\backup\\";
        public BackupDirectory() : this("") {
            
        }
        public BackupDirectory(string originalDirPath) {
            this.originalDirPath = originalDirPath;
            if (!Directory.Exists(originalDirPath)) {
                throw new ArgumentException("not exists <" + originalDirPath + ">");
            }
            backupDirPath = rootBackupDirectoryPath + Path.GetFileName(originalDirPath);
            watcher = new FileSystemWatcher(originalDirPath);
        }

        //対象のディレクトリに含まれる全ファイルをバックアップ
        public async Task backupAllFiles() {
            await copyDirectory(originalDirPath, backupDirPath);
        }

        private async Task copyDirectory(string srcPath, string destPath) {
            if(Directory.Exists(destPath)) {
                Directory.Delete(destPath, true);
            }
            Directory.CreateDirectory(destPath);

            try {
                foreach (var file in Directory.GetFiles(srcPath)) {


                    string dstFilePath = Path.Combine(destPath, Path.GetFileName(file));
                    //File.Copy(file, Path.Combine(destPath, Path.GetFileName(file)));
                    using (FileStream srcFile = File.Open(file, FileMode.Open)) {
                        if (File.Exists(dstFilePath)) {
                            using (FileStream dstFile = File.Open(dstFilePath, FileMode.Open)) {
                                await srcFile.CopyToAsync(dstFile);
                            }
                        } else {
                            using (FileStream dstFile = File.Create(dstFilePath)) {
                                await srcFile.CopyToAsync(dstFile);
                            }
                        }
                    }
                }
            } catch(UnauthorizedAccessException e) {

            }
            foreach(var dir in Directory.GetDirectories(srcPath)) {
                await copyDirectory(dir, Path.Combine(destPath, Path.GetFileName(dir)));
           }
        }

        //バックアップ元のファイルがバックアップ先に存在しないor古かった場合追加，更新する
        public async Task updateBackupDir() {
            Func<string, string, Task> update = null;
            update = async(src, dst) => {
                if (!Directory.Exists(dst)) {
                    Directory.CreateDirectory(dst);
                }
                try {
                    foreach (var srcFile in Directory.GetFiles(src)) {
                        string dstFilePath = Path.Combine(dst, Path.GetFileName(srcFile));

                        using (FileStream srcFileStream = File.Open(srcFile, FileMode.Open)) {
                            if (!File.Exists(dstFilePath)) {
                                using (FileStream dstFileStream = File.Create(dstFilePath)) {
                                    await srcFileStream.CopyToAsync(dstFileStream);
                                }
                            } else if (File.GetLastWriteTime(dstFilePath) < File.GetLastWriteTime(srcFile)) {
                                using (FileStream dstFileStream = File.Open(dstFilePath, FileMode.Open)) {
                                    await srcFileStream.CopyToAsync(dstFileStream);
                                }
                            }
                        }
                    }
                } catch(UnauthorizedAccessException e) {

                }
                foreach (var dir in Directory.GetDirectories(src)) {
                    await update(dir, Path.Combine(dst, Path.GetFileName(dir)));
                }
                
            };

            await update(originalDirPath, backupDirPath);
        }
        
        //バックアップ先のファイルがバックアップ元にない場合は，削除する
        //不要なファイルの削除
        public void deleteDiffBackupFiles() {
            Action<string, string> delete = null;
            delete = (srcDir, dstDir) => {
                if (!Directory.Exists(srcDir)) {
                    Directory.Delete(dstDir, true); //ディレクトリごと削除して終わり
                } else {
                    foreach (var dstFile in Directory.GetFiles(dstDir)) {
                        if (!File.Exists(Path.Combine(srcDir, Path.GetFileName(dstFile)))) {
                            File.Delete(dstFile);
                        }
                    }
                    foreach(var dir in Directory.GetDirectories(dstDir)) {
                        delete(Path.Combine(srcDir, Path.GetFileName(dir)), dir);
                    }
                }                    
            };

            delete(originalDirPath, backupDirPath);
            
        }
    }

    

}
