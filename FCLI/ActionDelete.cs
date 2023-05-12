using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CommandLine;

namespace FCLI
{
    partial class Program
    {
        static void actionDelete(ActionDeleteParams parms)
        {
            if (!File.Exists(parms.FileName))
            {
                Console.WriteLine("Container not found: {0}", parms.FileName);
                return;
            }

            using var c = new FileContainer.PersistentContainer(parms.FileName);
            var       r = c.Delete(parms.EntryName);
            if (r.Any())
                Console.WriteLine("No entries found");
            else
                foreach (var item in r.OrderBy(p => p))
                    Console.WriteLine("{0}: {1}", "Deleted", item);
        }
    }

    [ExcludeFromCodeCoverage]
    [Verb("delete", HelpText = "Delete of entries")]
    public class ActionDeleteParams
    {
        [Option('f', "file", Required      = true, HelpText = "File name of existing container")]                         public string FileName  { get; set; }
        [Option('e', "entryName", Required = true, HelpText = "Name or mask entry names for delete (* and ? supported)")] public string EntryName { get; set; }
    }
}