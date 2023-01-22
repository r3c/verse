using System.Collections.Generic;
using System.Linq;

namespace Verse.Schemas.Protobuf.Definition
{
    internal struct ProtoBinding
    {
    	private static readonly ProtoBinding[] EmptyBindings = new ProtoBinding[0];

        public static readonly ProtoBinding Empty = new ProtoBinding(string.Empty, ProtoType.Undefined);

        public readonly ProtoBinding[] Fields;

        public readonly string Name;

        public readonly ProtoType Type;

        public ProtoBinding(string name, IEnumerable<ProtoBinding> fields)
        {
            Fields = fields.ToArray();
            Name = name;
            Type = ProtoType.Custom;
        }

        public ProtoBinding(string name, ProtoType type)
        {
            Fields = EmptyBindings;
            Name = name;
            Type = type;
        }
    }
}
