using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Verse.Schemas
{
    public struct JSONSettings
    {
        public readonly Encoding Encoding;
        public readonly bool IgnoreNull;

        public JSONSettings(Encoding encoding, bool ignoreNull)
        {
            this.Encoding = encoding;
            this.IgnoreNull = ignoreNull;
        }
    }
}
