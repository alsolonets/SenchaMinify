using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;
using Microsoft.Ajax.Utilities;
using SenchaMinify.Library;

namespace SenchaMinify.Web
{
    /// <summary>
    /// Sencha bundle file
    /// </summary>
    public class SenchaBundleFile : SenchaFileNode
    {
        public BundleFile File { get; set; }

        /// <summary>
        /// Constructor with bundle file
        /// </summary>
        /// <param name="file">Sencha source file</param>
        /// <param name="parseCode">Parse to get raw code</param>
        public SenchaBundleFile(BundleFile file, bool parseCode)
            :base()
        {
            this.File = file;

            using (var sr = new StreamReader(file.VirtualFile.Open()))
            {
                this.Content = sr.ReadToEnd();
            }

            if (parseCode)
            {
                var parser = this.CreateParser(this.Content);
                var parsed = parser.Parse(parser.Settings);
                this.Content = parsed.ToCode();
            }
        }

        /// <summary>
        /// Create parser instance
        /// </summary>
        protected virtual JSParser CreateParser(string content)
        {
            var parser = new JSParser(content);
            parser.Settings.IgnoreAllErrors = true;
            parser.Settings.MinifyCode = true;
            return parser;
        }
    }
}