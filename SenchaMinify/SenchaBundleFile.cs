using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;

namespace SenchaMinify
{
    /// <summary>
    /// Sencha bundle file
    /// </summary>
    public class SenchaBundleFile : SenchaFile
    {
        public BundleFile File { get; set; }

        /// <summary>
        /// Constructor with bundle file
        /// </summary>
        /// <param name="file">Sencha source file</param>
        public SenchaBundleFile(BundleFile file)
            :base(file)
        {
            this.File = file;
        }

        public override string ToString()
        {
            return File.VirtualFile.Name;
        }
    }
}