using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static FileSystemVisitor.Lib.FileSystemVisitor;

namespace FileSystemVisitor.Lib
{
    public class FileSystemVisitor
    {
        private bool _interrupt = false;
        public delegate void Notifier(FileSystemVisitor o, FileSystemVisitorEventArgs e);
        public event Notifier SearchStart;
        public event Notifier SearchFinish;
        public event Notifier FileFound;
        public event Notifier DirectoryFound;
        public event Notifier FilteredFileFound;
        public event Notifier FilteredDirectoryFound;
        private List<FileSystemInfo> _all;
        private IEnumerable<DirectoryInfo> _folders { get => _all.Where(c => c as DirectoryInfo != null).Select(c => (DirectoryInfo)c); }
        private IEnumerable<FileInfo> _files { get => _all.Where(c => c as FileInfo != null).Select(c => (FileInfo)c); }

        Func<IEnumerable<FileSystemInfo>, IOrderedEnumerable<FileSystemInfo>> sortingFunc = null;
        public FileSystemVisitor() : this(null, null, null) { }
        public FileSystemVisitor(DirectoryInfo entryPiont, Func<IEnumerable<FileSystemInfo>, IOrderedEnumerable<FileSystemInfo>> func, Notifier notifier = null)
        {
            InvokeEvent(SearchStart, Events.SearchStart);
            this.sortingFunc = func;
            SubscribeDelegate(notifier);
            Initialize(entryPiont);
            ImplementSorting();
        }

        private void SubscribeDelegate(Notifier notifier)
        {
            if (notifier == null)
                return;
            SearchStart += notifier;
            SearchFinish += notifier;
            FileFound += notifier;
            DirectoryFound += notifier;
            FilteredFileFound += notifier;
            FilteredDirectoryFound += notifier;
        }
        public void Interrupt()
        {
            lock (this)
                this._interrupt = true;
        }
        public void Exclude(FileSystemInfo dir)
        {
            if (dir == null)
                throw new ArgumentNullException("dir");
            _all.Remove(dir);
        }
        private void ImplementSorting()
        {
            if (sortingFunc == null)
                return;
            this._all = sortingFunc(this._all).ToList();
            _all.ForEach(c => {
                if (c as DirectoryInfo != null)
                    InvokeEvent(FilteredDirectoryFound, Events.FilteredDirectoryFound, c);
                else
                    InvokeEvent(FilteredFileFound, Events.FilteredFileFound, c);
            });
        }
        private void Initialize(DirectoryInfo entryPoint = null)
        {
            InvokeEvent(SearchStart, Events.SearchStart);
            this._all = new List<FileSystemInfo>();
            if (entryPoint == null)
            {
                foreach (var dir in Directory.GetLogicalDrives())
                    VisitDirRecursively(new DirectoryInfo(dir));
            }
            else
                VisitDirRecursively(entryPoint);
            InvokeEvent(SearchFinish, Events.SearchFinish);
        }
        private void VisitDirRecursively(DirectoryInfo dir)
        {
            if (_interrupt)
                return;
            foreach (var temp in dir.GetDirectories())
                try
                {
                    VisitDirRecursively(temp);
                }
                catch { }
            this._all.Add(dir);
            InvokeEvent(DirectoryFound, Events.DirectoryFound, dir);
            this._all.AddRange(dir.GetFiles());
            dir.GetFiles().ToList().ForEach(c => InvokeEvent(FileFound, Events.FileFound, c));
        }
        public IEnumerable<DirectoryInfo> GetAllFolders()
        {
            foreach (var temp in _folders)
                yield return temp;
        }
        public IEnumerable<FileInfo> GetAllFiles()
        {
            foreach (var temp in _files)
                yield return temp;
        }
        private void InvokeEvent(Notifier notifier , Events ev, FileSystemInfo obj= null)
        {
            if (notifier == null)
                return;
            notifier.Invoke(this, new FileSystemVisitorEventArgs(ev, obj));
        }
    }
}
