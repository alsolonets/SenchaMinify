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
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            var senchaBundleFiles = files.Select(f => new SenchaBundleFile(f));
            var ordered = OrderFiles(senchaBundleFiles);
            var result = ordered.Select(ef => ef.File);
            return result;
        }
    }
}