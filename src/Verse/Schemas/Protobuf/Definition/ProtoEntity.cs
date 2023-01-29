using System;
using System.Collections.Generic;

namespace Verse.Schemas.Protobuf.Definition;

internal struct ProtoEntity
{
    public readonly ProtoContainer Container;

    public readonly List<ProtoEntity> Entities;

    public readonly List<ProtoField> Fields;

    public readonly List<ProtoLabel> Labels;

    public readonly string Name;

    public ProtoEntity(ProtoContainer container, string name)
    {
        Container = container;
        Entities = new List<ProtoEntity>();
        Fields = new List<ProtoField>();
        Labels = new List<ProtoLabel>();
        Name = name;
    }

    public ProtoBinding[] Resolve(string name)
    {
        int index = Entities.FindIndex(d => d.Name == name);

        if (index < 0)
            throw new ResolverException("can't find message '{0}'", name);

        var entity = Entities[index];

        return ResolveEntity(entity, new[] { this, entity });
    }

    private ProtoBinding[] ResolveEntity(ProtoEntity entity, IEnumerable<ProtoEntity> parents)
    {
        var bindings = new ProtoBinding[0];

        foreach (var field in entity.Fields)
        {
            if (bindings.Length <= field.Number)
                Array.Resize(ref bindings, field.Number + 1);

            bindings[field.Number] = ResolveField(field, parents);
        }

        return bindings;
    }

    private ProtoBinding ResolveField(ProtoField field, IEnumerable<ProtoEntity> parents)
    {
        if (field.Reference.Type != ProtoType.Custom)
            return new ProtoBinding(field.Name, field.Reference.Type);

        for (var stack = new Stack<ProtoEntity>(parents); stack.Count > 0; stack.Pop())
        {
            var entity = stack.Peek();
            var found = true;
            var match = new List<ProtoEntity>(stack);

            foreach (var name in field.Reference.Names)
            {
                var index = entity.Entities.FindIndex(e => e.Name == name);

                if (index < 0)
                {
                    found = false;

                    break;
                }

                entity = entity.Entities[index];
                match.Add(entity);
            }

            if (found)
            {
                if (entity.Container == ProtoContainer.Enum)
                    return new ProtoBinding(field.Name, ProtoType.Int32);

                return new ProtoBinding(field.Name, ResolveEntity(entity, match));
            }
        }

        throw new ResolverException("field '{0}' has undefined type '{1}'", field.Name, string.Join(".", field.Reference.Names));
    }
}