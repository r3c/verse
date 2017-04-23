using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Schemas.Protobuf.Definition
{
    struct ProtoBinding
    {
    	private static readonly ProtoBinding[] EmptyBindings = new ProtoBinding[0];

        public static readonly ProtoBinding Empty = new ProtoBinding(string.Empty, ProtoType.Undefined);

        public readonly ProtoBinding[] Fields;

        public readonly string Name;

        public readonly ProtoType Type;

        public ProtoBinding(string name, IEnumerable<ProtoBinding> fields)
        {
            this.Fields = fields.ToArray();
            this.Name = name;
            this.Type = ProtoType.Custom;
        }

        public ProtoBinding(string name, ProtoType type)
        {
            this.Fields = ProtoBinding.EmptyBindings;
            this.Name = name;
            this.Type = type;
        }
    }
}
