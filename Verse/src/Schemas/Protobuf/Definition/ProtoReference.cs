using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Schemas.Protobuf.Definition
{
    struct ProtoReference
    {
        public readonly string[] Names;

        public readonly ProtoType Type;

        public ProtoReference(IEnumerable<string> names)
        {
            this.Names = names.ToArray();
            this.Type = ProtoType.Custom;
        }

        public ProtoReference(ProtoType type)
        {
            this.Names = null;
            this.Type = type;
        }
    }
}
