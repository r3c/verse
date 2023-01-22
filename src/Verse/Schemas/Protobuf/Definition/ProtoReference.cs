using System.Collections.Generic;
using System.Linq;

namespace Verse.Schemas.Protobuf.Definition
{
    internal struct ProtoReference
    {
        public readonly string[] Names;

        public readonly ProtoType Type;

        public ProtoReference(IEnumerable<string> names)
        {
            Names = names.ToArray();
            Type = ProtoType.Custom;
        }

        public ProtoReference(ProtoType type)
        {
            Names = null;
            Type = type;
        }
    }
}
