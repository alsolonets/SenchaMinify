using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Microsoft.Ajax.Utilities;

namespace SenchaMinify
{
    public class SenchaBundle : ScriptBundle
    {
        public SenchaBundle(string virtualPath)
            : this(virtualPath, null)
        {

        }

        public SenchaBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath)
        {
            this.Orderer = new SenchaBundleOrderer();
        }
    }
}