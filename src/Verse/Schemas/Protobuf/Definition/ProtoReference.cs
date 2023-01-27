using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Schemas.Protobuf.Definition;

internal struct ProtoReference
{
    public readonly IReadOnlyList<string> Names;

    public readonly ProtoType Type;

    public ProtoReference(IEnumerable<string> names)
    {
        Names = names.ToList();
        Type = ProtoType.Custom;
    }

    public ProtoReference(ProtoType type)
    {
        Names = Array.Empty<string>();
        Type = type;
    }
}