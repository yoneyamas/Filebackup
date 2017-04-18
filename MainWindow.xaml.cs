using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Filebackup {
    
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        BackupFile backup;
        ObservableCollection<Node> backupDirNode;

        public MainWindow() {
            
            InitializeComponent();
            
            backup = new BackupFile();
            backup.loadBackupDir();

            backupDirNode = new ObservableCollection<Node>();
            foreach (var x in backup.backupDir) {
                backupDirNode.Add(new Node(x.BackupDirPath));
            }
            this.BackupFileTree.ItemsSource = backupDirNode;

            List<Node> topNodes2 = new List<Node>();
            List<string> drives = new List<string>(Directory.GetLogicalDrives());
            foreach(var d in drives) {
                if (Directory.Exists(d)) {     //有効なドライブかどうか調べる
                    topNodes2.Add(new Node(d));
                }
            }
            this.FileTree.ItemsSource = topNodes2;

            Closing += Window_Closing;
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            await backup.allSyncBackupDir();            
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e) {
            var folderDialog = new FolderBrowserDialog();

            folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            var result = folderDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK) {
                await backup.addBackupDirectory(folderDialog.SelectedPath, backupDirNode);                
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            await backup.allSyncBackupDir();
            backup.writeBackupDir();

        }

        private async void backupList_Drop(object sender, System.Windows.DragEventArgs e) {
            string[] directories = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if(directories != null) {
                foreach(var dir in directories) {
                    await backup.addBackupDirectory(dir, backupDirNode);
                }
            }
        }

    
    }
}
