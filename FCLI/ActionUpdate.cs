using System;
using System.IO;
using CommandLine;
using FileContainer;

namespace FCLI
{
    partial class Program
    {
        static void actionUpdate(ActionUpdateParams parms)
        {
            if (!File.Exists(parms.FileName))
            {
                Console.WriteLine("Container not found: {0}", parms.FileName);
                return;
            }
            
            if (!File.Exists(parms.SourceFile))
            {
                Console.WriteLine("Source file not exists: {0}", parms.SourceFile);
                return;
            }

            var entryName = string.IsNullOrEmpty(parms.EntryName)
                ? Path.GetFileName(parms.SourceFile)
                : parms.EntryName;
            
            using var c = new FileContainer.PersistentContainer(parms.FileName);
            var       r = c.Put(entryName, File.ReadAllBytes(parms.SourceFile));
            Console.WriteLine("{0}: {1}", r == PutAppendResult.Created ? "Created" : "Updated", parms.SourceFile);
        }
    }

    [Verb("update", HelpText = "Add or update container")]
    public class ActionUpdateParams
    {
        [Option('f', "file", Required       = true, HelpText  = "File name of existing container")]                                    public string FileName   { get; set; }
        [Option('e', "entryName", Required  = false, HelpText = "Name of new entry. Will used file name if entry name not specified")] public string EntryName  { get; set; }
        [Option('s', "sourceFile", Required = true, HelpText  = "Source file name")]                                                   public string SourceFile { get; set; }

    }
}