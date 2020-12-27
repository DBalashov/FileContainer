using System;
using System.IO;
using System.Linq;
using CommandLine;

namespace FCLI
{
    partial class Program
    {
        static void actionDir(ActionDirParams parms)
        {
            if (!File.Exists(parms.FileName))
            {
                Console.WriteLine("Container not found: {0}", parms.FileName);
                return;
            }

            using var c       = new FileContainer.PersistentReadonlyContainer(parms.FileName);
            var       entries = string.IsNullOrEmpty(parms.Mask) ? c.Find() : c.Find(parms.Mask);
            if (!entries.Any())
            {
                Console.WriteLine("Container is empty");
                return;
            }

            Console.WriteLine("Founded entries: {0}", entries.Length);
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("    Length   Modified time        First page    Entry name");
            Console.WriteLine("--------------------------------------------");
            var summaryLength = 0;
            foreach (var item in entries.OrderBy(p => p.Name))
            {
                Console.WriteLine("{0,10}   {1:dd.MM.yyyy HH:mm:ss}     {2,7}  {3}", item.Length, item.Modified.ToLocalTime(), item.FirstPage, item.Name);
                summaryLength += item.Length;
            }
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("{0,10} bytes total", summaryLength);
        }
    }

    [Verb("dir", HelpText = "Show directory of container")]
    public class ActionDirParams
    {
        [Option('f', "file", Required = true, HelpText  = "File name of container")]             public string FileName { get; set; }
        [Option('m', "mask", Required = false, HelpText = "Mask for names (* and ? supported)")] public string Mask     { get; set; }
    }
}