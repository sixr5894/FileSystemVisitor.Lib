using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileSystemVisitor.Lib;

namespace FileVisitorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var temp = new FileSystemVisitor.Lib.FileSystemVisitor(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent, c => c.OrderBy(t => t.CreationTime), Notify);

            Console.ReadLine();

        }
        static void InterruptLater( FileSystemVisitorEventArgs arg , FileSystemVisitor.Lib.FileSystemVisitor sender)
        {
            Thread.Sleep(500);
            arg.InterruptSearch(sender);
        }
        static void Notify(FileSystemVisitor.Lib.FileSystemVisitor sender, FileSystemVisitorEventArgs arg)
        {
            switch (arg._event)
            {
                case  Events.SearchStart :
                    Console.WriteLine("SearchStart");
                    Task.Run(() => InterruptLater(arg, sender ));
                    break;
                case Events.SearchFinish:
                    Console.WriteLine("SearchFinish");
                    break;
                case Events.FileFound:
                    Console.WriteLine("FileFound");
                    break;
                case Events.DirectoryFound:
                    Console.WriteLine("DirectoryFound");
                    break;
                case Events.FilteredFileFound:
                    Console.WriteLine("FilteredFileFound");
                    break;
                case Events.FilteredDirectoryFound:
                    Console.WriteLine("FilteredDirectoryFound");
                    break;
            }
            
        }
    }
}
