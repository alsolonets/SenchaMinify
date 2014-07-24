using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using Plossum.CommandLine;
using SenchaMinify;

namespace SenchaMinify.Cmd
{
    class Program
    {
        static int Main(string[] args)
        {
            var startTime = DateTime.Now;
            int errorCode;
            Options options;

            if (!TryGetOptions(out options, out errorCode))
            {
                return errorCode;
            }

            // Files to sort
            var files = new List<FileInfo>();

            // Include directories
            if (options.Include.Any())
            {
                var dirs = options.Include.Select(d => new DirectoryInfo(d)).ToList();
                if (!CheckExists(dirs))
                {
                    return -1;
                }
                dirs.ForEach(d => files.AddRange(d.GetFiles(options.SearchPattern, SearchOption.TopDirectoryOnly)));
            }

            // Include with subdirectories
            if (options.IncludeRecursive.Any())
            {
                var dirs = options.IncludeRecursive.Select(d => new DirectoryInfo(d)).ToList();
                if (!CheckExists(dirs))
                {
                    return -1;
                }
                dirs.ForEach(d => files.AddRange(d.GetFiles(options.SearchPattern, SearchOption.AllDirectories)));
            }

            // Exclude files
            if (options.Exclude.Any())
            {
                var dirs = options.Exclude.Select(d => new DirectoryInfo(d)).ToList();
                files = files.Where(f => !dirs.Any(d => f.DirectoryName.StartsWith(d.FullName))).ToList();
            }

            // Always order files
            IList<SenchaFileWrapper> ordered;

            try
            {
                ordered = OrderFiles(files).ToList();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error ordering files. {0}", e.Message);
                return -1;
            }

            // Writing ordered files to console
            if (options.Sort)
            {
                ordered.ForEach(f =>
                {
                    Console.WriteLine(f.File.FullName);
                });
            }

            // Minification and concatenation
            if (options.Concat || options.Minify)
            {
                var content = String.Join(Environment.NewLine, ordered.Select(f => f.Content));

                if (options.Minify)
                {
                    try
                    {
                        content = new Minifier().MinifyJavaScript(content);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("Minifying error. {0}", e.Message);
                        return -1;
                    }
                }

                try
                {
                    using (var writer = new StreamWriter(options.OutputPath))
                    {
                        writer.Write(content);
                    }

                    Console.WriteLine("Successfully wrote file in {0}ms: {1} ({2} bytes)",
                        (int)(DateTime.Now - startTime).TotalMilliseconds, options.OutputPath, content.Length);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Error writing output file. {0}", e.Message);
                    return -1;
                }
            }

            return 0;
        }

        public static bool CheckExists(IEnumerable<DirectoryInfo> dirs)
        {
            if (dirs.Any(d => !d.Exists))
            {
                dirs.Where(d => !d.Exists).ToList()
                    .ForEach(d => Console.Error.WriteLine("{0} not exists", d.FullName));
                return false;
            }
            else
            {
                return true;
            }
        }

        public static IEnumerable<SenchaFileWrapper> OrderFiles(IEnumerable<FileInfo> files)
        {
            var senchaFiles = files.Select(f => new SenchaFileWrapper(f));
            return new SenchaOrderer().OrderFiles(senchaFiles);
        }

        public static bool TryGetOptions(out Options outOptionsOut, out int outErrorCode)
        {
            var options = new Options();
            var parser = new CommandLineParser(options);
            outErrorCode = 0;
            outOptionsOut = options;

            try
            {
                parser.Parse();

                if (options.Help)
                {
                    var appName = Path.GetFileName(parser.ExecutablePath);

                    Console.WriteLine(Environment.NewLine + "Usage" + Environment.NewLine);

                    Console.WriteLine("Minify an application:");
                    Console.WriteLine(appName + " -r /path/to/my/ext/app -m -o app.min.js");
                    Console.WriteLine();

                    Console.WriteLine("Sort application files and display their names:");
                    Console.WriteLine(appName + " -r /path/to/my/ext/app -s");
                    Console.WriteLine();

                    Console.WriteLine("Sort application files and display their names except a directory, concat and save to file:");
                    Console.WriteLine(appName + " --include-recursive /path/to/my/ext/app --exclude /path/to/my/ext/app/extra --sort --concat --out app.all.js");
                    Console.WriteLine();

                    Console.WriteLine(parser.UsageInfo.ToString(Console.BufferWidth, false));
                    outErrorCode = 0;
                }
                else if (parser.HasErrors)
                {
                    Console.Error.WriteLine(parser.UsageInfo.ToString(Console.BufferWidth, true));
                    outErrorCode = -1;
                }
                else if ((options.Concat || options.Minify) && String.IsNullOrEmpty(options.OutputPath))
                {
                    throw new InvalidOptionValueException("Must specify file output path");
                }
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                if (tie.InnerException != null)
                {
                    ShowParseException(parser, tie.InnerException.Message);
                }
                else
                {
                    ShowParseException(parser, tie.Message);
                }
                outErrorCode = -1;
            }
            catch (ParseException e)
            {
                ShowParseException(parser, e.Message);
                outErrorCode = -1;
            }

            return outErrorCode == 0;
        }

        public static void ShowParseException(CommandLineParser parser, string message)
        {
            Console.Error.WriteLine(Environment.NewLine + "Errors:");
            Console.Error.WriteLine("\t* {0}" + Environment.NewLine, message);
            Console.Error.WriteLine(parser.UsageInfo.ToString(Console.BufferWidth, false));
        }
    }
}
