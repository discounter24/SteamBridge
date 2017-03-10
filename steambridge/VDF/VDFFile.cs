using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace steambridge.VDF
{
    public class VDFFile : NestedElementFile
    {
        public VDFFile(string file_name) : base(file_name) { }


        protected static string DeEscapeString(string value)
        {
            return value.Replace(@"\\", @"\");
        }
    }
}
