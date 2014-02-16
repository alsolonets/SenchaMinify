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
            :this()
        {
            this.Content = File.ReadAllText(file.FullName);
        }

        public SenchaFile(BundleFile file)
            :this()
        {
            using (var sr = new StreamReader(file.VirtualFile.Open()))
            {
                this.Content = sr.ReadToEnd();
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
                .Select(cn => cn.Arguments.OfType<ObjectLiteral>().First())
                .Select(arg => new SenchaClass(arg) { IsApplication = true });

            var extDefines = this.RootBlock.OfType<CallNode>()
                .Where(cn => cn.Arguments.OfType<ConstantWrapper>().Any())
                .Where(cn => cn.Arguments.OfType<ObjectLiteral>().Any())
                .Where(cn =>
                {
                    var code = cn.Children.First().Context.Code;
                    return code == "Ext.define" || code == "Ext.override";
                })
                .Select(cn =>
                {
                    var className = cn.Arguments.OfType<ConstantWrapper>().First().Value.ToString();
                    var config = cn.Arguments.OfType<ObjectLiteral>().First();
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
                    .Intersect(this.Classes.SelectMany(tc => tc.DependencyClasses))
                    .Any()
                );
        }
    }
}