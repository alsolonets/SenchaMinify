using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Optimization;
using Microsoft.Ajax.Utilities;

namespace SenchaMinify
{
    /// <summary>
    /// Sencha application source file wrapper
    /// </summary>
    public class SenchaFile
    {
        public enum SortColor { White, Gray, Black };

        /// <summary>
        /// Sorting color. Used by topological sort.
        /// </summary>
        public SortColor Color { get; set; }

        /// <summary>
        /// File full name including path
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Class definitions
        /// </summary>
        public IEnumerable<SenchaClass> Classes
        {
            get
            {
                return _Classes ?? (_Classes = GetClasses());
            }
        }
        protected IEnumerable<SenchaClass> _Classes;

        /// <summary>
        /// File content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Root JS file block
        /// </summary>
        public virtual Block RootBlock
        {
            get
            {
                return _RootBlock ?? (_RootBlock = Parser.Parse(Content));
            }
        }
        private Block _RootBlock;

        /// <summary>
        /// JS parser
        /// </summary>
        protected JSParser Parser
        {
            get
            {
                return _Parser ?? (_Parser = CreateParser());
            }
        }
        private JSParser _Parser;

        /// <summary>
        /// Get a collection of file dependencies. Should be used after FillDependencies call.
        /// </summary>
        public virtual IEnumerable<SenchaFile> Dependencies { get; set; }

        private SenchaFile()
        {
            this.Color = SortColor.White;
        }

        public SenchaFile(string content)
            :this()
        {
            this.Content = content;
        }

        public SenchaFile(FileInfo file)
            :this(GetContent(file))
        {
        
        }

        public SenchaFile(BundleFile file)
            :this(GetContent(file))
        {
        
        }

        /// <summary>
        /// Returns content from a regular file
        /// </summary>
        protected static string GetContent(FileInfo file)
        {
            return File.ReadAllText(file.FullName);
        }

        /// <summary>
        /// Returns content from a bundle file
        /// </summary>
        protected static string GetContent(BundleFile file)
        {
            using (var sr = new StreamReader(file.VirtualFile.Open()))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Create parser instance
        /// </summary>
        protected virtual JSParser CreateParser()
        {
            var parser = new JSParser();
            parser.Settings.IgnoreAllErrors = true;
            parser.Settings.MinifyCode = true;
            return parser;
        }

        /// <summary>
        /// Get class definitions
        /// </summary>
        protected virtual IEnumerable<SenchaClass> GetClasses()
        {
            var extApps = this.RootBlock.OfType<CallNode>()
                .Where(cn => cn.Children.Any())
                .Where(cn => cn.Children.First().Context.Code == "Ext.application")
                .Where(cn => cn.Arguments.Take(1).OfType<ObjectLiteral>().Any()) // only apps with a config object
                .Select(cn => cn.Arguments.OfType<ObjectLiteral>().First())
                .Select(config => new SenchaClass(config) 
                { 
                    IsApplication = true 
                });

            var extDefines = this.RootBlock.OfType<CallNode>()
                .Where(cn => cn.Arguments.Take(1).OfType<ConstantWrapper>().Any()) // where first argument is the class name
                .Where(cn =>
                {
                    var code = cn.Children.First().Context.Code;
                    return code == "Ext.define" || code == "Ext.override";
                })
                .Select(cn =>
                {
                    var className = cn.Arguments.OfType<ConstantWrapper>().First().Value.ToString();
                    var config = cn.Arguments.OfType<ObjectLiteral>().FirstOrDefault();
                    return new SenchaClass(config) { ClassName = className };
                });            

            foreach (var cls in extApps.Union(extDefines))
            {
                yield return cls;
            }
        }

        /// <summary>
        /// Fills the Dependencies collection using DependencyClasses
        /// </summary>
        /// <param name="allFiles">All files in the bundle</param>
        public virtual void FillDependencies(IEnumerable<SenchaFile> allFiles)
        {
            Dependencies = allFiles
                .Where(f => f.Classes
                    .Select(c => c.ClassName)
                    .Intersect(this.Classes.SelectMany(tc => tc.DependencyClassNames))
                    .Any()
                );
#if DEBUG
            var missingDependencies = GetMissingDependencies(allFiles).ToList();
            if (missingDependencies.Count > 0)
            {
                string notFound = String.Join(", ", missingDependencies);
                System.Diagnostics.Debug.WriteLine("SenchaMinify. {0}: Cannot find dependencies: {1}", this.ToString(), notFound);
            }
#endif
        }

        /// <summary>
        /// Gets a collection of dependant class names that are not found in 'allFiles' collection
        /// </summary>
        /// <param name="allFiles">All files in the bundle</param>
        public virtual IEnumerable<string> GetMissingDependencies(IEnumerable<SenchaFile> allFiles)
        {
            return this.Classes
                .SelectMany(c => c.DependencyClassNames)
                .Where(c => !allFiles.SelectMany(f => f.Classes.Select(fc => fc.ClassName)).Contains(c))
                .Where(c => !c.StartsWith("Ext."));     // may be not good if you have custom 'Ext.ux' classes
        }
    }
}