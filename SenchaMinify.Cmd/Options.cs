using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plossum.CommandLine;

namespace SenchaMinify.Cmd
{
    [CommandLineManager(ApplicationName = "SenchaMinify.Cmd", Description = "SenchaMinify Command Line Interface", EnabledOptionStyles = OptionStyles.Unix | OptionStyles.Group)]
    [CommandLineOptionGroup("action", Name = "Actions", Require = OptionGroupRequirement.AtLeastOne)]
    [CommandLineOptionGroup("include", Name = "Include Directories", Require = OptionGroupRequirement.AtLeastOne)]
    public class Options
    {
        [CommandLineOption(Name = "help", Aliases = "?", Description = "Displays this help.")]
        public bool Help { get; set; }


        [CommandLineOption(Name = "s", Aliases = "sort", Description = "Show sorted source file names.", GroupId = "action")]
        public bool Sort { get; set; }

        [CommandLineOption(Name = "c", Aliases = "concat", Description = "Concat input files. Cannot be used with -m option.", GroupId = "action")]
        public bool Concat
        {
            get
            {
                return _Concat;
            }
            set
            {
                if (value && Minify)
                {
                    throw new Plossum.CommandLine.InvalidOptionValueException("Minify and Concat options cannot be used together");
                }
                _Concat = value;
            }
        }
        private bool _Concat;

        [CommandLineOption(Name = "m", Aliases = "minify", Description = "Minify input files. Cannot be used with -c option.", GroupId = "action")]
        public bool Minify
        {
            get
            {
                return _Minify;
            }
            set
            {
                if (value && Concat)
                {
                    throw new Plossum.CommandLine.InvalidOptionValueException("Minify and Concat options cannot be used together");
                }
                _Minify = value;
            }
        }
        public bool _Minify;


        [CommandLineOption(Name = "i", Aliases = "include", Description = "Source files directory to include.", GroupId = "include")]
        public List<string> Include { get; set; }

        [CommandLineOption(Name = "r", Aliases = "include-recursive", Description = "Source files directory to include (with subdirectories).", GroupId = "include")]
        public List<string> IncludeRecursive { get; set; }

        [CommandLineOption(Name = "e", Aliases = "exclude", Description = "Exclude directory path. To be used with -r option.")]
        public List<string> Exclude { get; set; }


        [CommandLineOption(Name = "p", Description = "Search pattern. Defaults to '*.js'.")]
        public string SearchPattern { get; set; }

        [CommandLineOption(Name = "o", Aliases = "out", Description = "File output path. To be used with -c and -m options.")]
        public string OutputPath { get; set; }


        public Options()
        {
            this.Include = new List<string>();
            this.IncludeRecursive = new List<string>();
            this.Exclude = new List<string>();
            this.SearchPattern = "*.js";
        }
    }
}
