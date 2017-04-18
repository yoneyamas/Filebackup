using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Filebackup {
    class Node {
        public enum EType {
            File, Dir, Drive
        }

        public string Name { get; set; }

        public EType Type {
            get; private set;
        }

    
        public bool IsFile {
            get { return Type == EType.File; }
        }

        public bool IsDir {
            get { return Type == EType.Dir; }
        }

        public bool IsDrive {
            get { return Type == EType.Drive; }
        }
        
        public string FullPath {
            get; private set;
        }
        

        public List<Node> Children {
            get {
                //ファイルだったらサブフォルダandサブファイルは存在しないため，nullを返す
                if(Type == EType.File) {
                    return null;
                }

                List<Node> children = new List<Node>();

                try {
                    IEnumerable<string> dirList = Directory.GetDirectories(FullPath);
                    IEnumerable<string>  fileList = Directory.GetFiles(FullPath);

                    foreach (var p in dirList) {
                        children.Add(new Node(p));
                    }
                    foreach (var p in fileList) {
                        children.Add(new Node(p));
                    }
                } catch(UnauthorizedAccessException e) {

                }
                
                return children;

            }
        }

        public Node() : this("dummy") {

        }

        public Node(string fullPath) {
            FullPath = fullPath;

            if (File.Exists(fullPath)) {
                Type = EType.File;
                Name = Path.GetFileName(fullPath);
            } else if (Directory.Exists(fullPath)) {
                if (fullPath.Length == 3 && Regex.IsMatch(fullPath, @"[A-Z]:\\")) {
                    Type = EType.Drive;
                    Name = fullPath;
                } else {
                    Type = EType.Dir;
                    Name = Path.GetFileName(fullPath);
                }
            } else {
                throw new ArgumentException();
            }
        }

        


    }
}
