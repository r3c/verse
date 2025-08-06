using Verse.DecoderDescriptors.Tree;
using Verse.Formats.Protobuf;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf;

internal class ProtobufReaderDefinition<TEntity> : IReaderDefinition<ReaderState, ProtobufValue, int, TEntity>
{
    public ILookup<int, ReaderCallback<ReaderState, ProtobufValue, int, TEntity>> Lookup { get; }

    //private static readonly Reader<TEntity> emptyReader = new Reader<TEntity>(new ProtoBinding[0], false);

    private readonly ProtoBinding[] _bindings;

    //private readonly ReaderCallback<ReaderState, ProtobufValue, TEntity>[] fields;

    private readonly bool _rejectUnknown;

    public ProtobufReaderDefinition(ProtoBinding[] bindings, bool rejectUnknown)
    {
        Lookup = null!; // FIXME

        _bindings = bindings;
        //this.fields = new ReaderCallback<ReaderState, ProtobufValue, TEntity>[bindings.Length];
        _rejectUnknown = rejectUnknown;
    }

    public IReaderDefinition<ReaderState, ProtobufValue, int, TOther> Create<TOther>()
    {
        return new ProtobufReaderDefinition<TOther>(_bindings, _rejectUnknown);
    }
    /*
            public override TreeReader<ReaderState, TField, ProtobufValue> HasField<TField>(string name, ReaderCallback<ReaderState, TEntity> enter)
            {
                int index = Array.FindIndex(this.bindings, binding => binding.Name == name);

                if (index < 0)
                    throw new ArgumentOutOfRangeException("name", name, "field doesn't exist in proto definition");

                this.fields[index] = enter;

                return new Reader<TField>(this.bindings[index].Fields, this.rejectUnknown);
            }
    */
}