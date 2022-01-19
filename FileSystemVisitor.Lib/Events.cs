using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemVisitor.Lib
{
    public enum Events : short
    {
        SearchStart,
        SearchFinish,
        FileFound,
        DirectoryFound,
        FilteredFileFound,
        FilteredDirectoryFound
    }
}
