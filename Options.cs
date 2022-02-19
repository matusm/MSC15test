﻿using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace MSC15test
{
    public class Options
    {
        [Option('n', "number", DefaultValue = 10, HelpText = "Number of samples.")]
        public int MaximumSamples { get; set; }

        [Option("comment", DefaultValue = "---", HelpText = "User supplied comment string.")]
        public string UserComment { get; set; }

        [Option("logfile", DefaultValue = "MSC15test.log", HelpText = "Log file name.")]
        public string LogFileName { get; set; }

        [Option("prefix", DefaultValue = "", HelpText = "Prefix for spectrum files.")]
        public string SpecFilePrefix { get; set; }

        [Option('s', "skipdark", DefaultValue = false, HelpText = "Skip dark offset measurement at startup.")]
        public bool NoOffset { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string AppVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            HelpText help = new HelpText
            {
                Heading = new HeadingInfo($"{AppName}, version {AppVer}"),
                Copyright = new CopyrightInfo("Michael Matus", 2022),
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true
            };
            string preamble = "Program to operate a spectroradiometers by Gigahertz-Optik. It is controlled via its USB interface. " +
                "Measurement results are logged in a file.";
            help.AddPreOptionsLine(preamble);
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine($"Usage: {AppName} [options]");
            help.AddPostOptionsLine("");
            help.AddOptions(this);

            return help;
        }
    }
}
