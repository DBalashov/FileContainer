using System;
using CommandLine;
using CommandLine.Text;

namespace FCLI
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var parserResult = new Parser(with => with.HelpWriter = null)
                .ParseArguments<ActionCreateParams, ActionDirParams, ActionUpdateParams, ActionDeleteParams>(args);

            try
            {
                parserResult
                    .WithParsed<ActionCreateParams>(actionCreate)
                    .WithParsed<ActionDirParams>(actionDir)
                    .WithParsed<ActionUpdateParams>(actionUpdate)
                    .WithParsed<ActionDeleteParams>(actionDelete)
                    .WithNotParsed(errs =>
                    {
                        var helpText = HelpText.AutoBuild(parserResult, h =>
                        {
                            h.AdditionalNewLineAfterOption = false;
                            h.Heading                      = "FileContainer CLI";
                            h.Copyright                    = "Copyright (c) 2020";
                            h.MaximumDisplayWidth          = 999;
                            return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                        }, e => e);

                        Console.WriteLine(helpText);
                    });
            }
            catch (Exception e)
            {
                e = e.InnerException ?? e;
                Console.WriteLine(e.Message);
            }
        }
    }
}