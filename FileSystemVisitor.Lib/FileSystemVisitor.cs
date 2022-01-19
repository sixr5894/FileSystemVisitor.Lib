using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
                    FilteredDirFinded(c);
                else
                    FilteredFileFinded(c);
            });
        }
        private void Initialize(DirectoryInfo entryPoint = null)
        {
            SearchStarded();
            this._all = new List<FileSystemInfo>();
            if (entryPoint == null)
            {
                foreach (var dir in Directory.GetLogicalDrives())
                    VisitDirRecursively(new DirectoryInfo(dir));
            }
            else
                VisitDirRecursively(entryPoint);
            SearchFinished();
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
            DirFinded(dir);
            this._all.AddRange(dir.GetFiles());
            dir.GetFiles().ToList().ForEach(c => FileFinded(c));
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
        private void SearchStarded()
        {
            if (SearchStart == null)
                return;
            this.SearchStart.Invoke(this, new FileSystemVisitorEventArgs(Events.SearchStart));
        }
        private void SearchFinished()
        {
            if (SearchFinish == null)
                return;
            SearchFinish.Invoke(this, new FileSystemVisitorEventArgs(Events.SearchFinish));
        }
        private void FileFinded(FileSystemInfo obj)
        {
            if (FileFound == null)
                return;
            FileFound.Invoke(this, new FileSystemVisitorEventArgs(Events.FileFound, obj));
        }
        private void DirFinded(FileSystemInfo obj)
        {
            if (DirectoryFound == null)
                return;
            DirectoryFound.Invoke(this, new FileSystemVisitorEventArgs(Events.DirectoryFound, obj));
        }
        private void FilteredFileFinded(FileSystemInfo obj)
        {
            if (FilteredFileFound == null)
                return;
            FilteredFileFound.Invoke(this, new FileSystemVisitorEventArgs(Events.FilteredFileFound, obj));
        }
        private void FilteredDirFinded(FileSystemInfo obj)
        {
            if (FilteredDirectoryFound == null)
                return;
            FilteredDirectoryFound.Invoke(this, new FileSystemVisitorEventArgs(Events.FilteredDirectoryFound, obj));
        }
    }
}
