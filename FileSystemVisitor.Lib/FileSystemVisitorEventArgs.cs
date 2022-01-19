using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemVisitor.Lib
{
    public class FileSystemVisitorEventArgs : EventArgs
    {
        public FileSystemVisitorEventArgs(Events ev, FileSystemInfo obj = null)
        {
            this._event = ev;
            this.RelatedObj = obj;
        }
        public FileSystemInfo RelatedObj;
        public Events _event { get; }
        public void InterruptSearch(FileSystemVisitor visitor) => visitor.Interrupt();
        public void Exclude(FileSystemInfo file, FileSystemVisitor visitor) => visitor.Exclude(file);
    }
}
