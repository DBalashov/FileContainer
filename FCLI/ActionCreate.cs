using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommandLine;
using FileContainer;

namespace FCLI
{
    partial class Program
    {
        static void actionCreate(ActionCreateParams parms)
        {
            if (File.Exists(parms.FileName))
                File.Delete(parms.FileName);

            using var c = new FileContainer.PersistentContainer(parms.FileName, new PersistentContainerSettings(parms.PageSize));
            Console.WriteLine("Created: {0}", parms.FileName);
        }
    }

    [ExcludeFromCodeCoverage]
    [Verb("create", HelpText = "Add or update container")]
    public class ActionCreateParams
    {
        [Option('f', "file", Required     = true, HelpText  = "File name of container")]    public string FileName { get; set; }
        [Option('p', "pageSize", Required = false, HelpText = "Page size", Default = 4096)] public int    PageSize { get; set; }
    }
}