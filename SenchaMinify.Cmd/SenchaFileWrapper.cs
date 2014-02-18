using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SenchaMinify.Cmd
{
    /// <summary>
    /// Wrapper just for getting FileInfo back
    /// </summary>
    public class SenchaFileWrapper : SenchaFile
    {
        public FileInfo File { get; set; }

        public SenchaFileWrapper(FileInfo file)
            :base(file)
        {
            this.File = file;
        }

        public override string ToString()
        {
            return File.Name;
        }
    }
}
