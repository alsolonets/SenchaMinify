using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;

namespace SenchaMinify
{
    public class SenchaBundleOrderer : SenchaOrderer, IBundleOrderer
    {
        /// <summary>
        /// Get a collection of SenchaBundleFile 
        /// </summary>
        /// <param name="files">Source files collection</param>
        /// <returns>Collection of SenchaBundleFile</returns>
        public virtual IEnumerable<SenchaBundleFile> GetSenchaBundleFiles(IEnumerable<BundleFile> files)
        {
            var result = files.Select(f => new SenchaBundleFile(f)).ToList();
            return result;
        }

        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            var senchaFiles = GetSenchaBundleFiles(files);
            var ordered = OrderFiles(senchaFiles);
            var result = ordered.Select(ef => ef.File);
            return result;
        }
    }
}