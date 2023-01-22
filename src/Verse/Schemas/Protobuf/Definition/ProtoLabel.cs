
namespace Verse.Schemas.Protobuf.Definition
{
    internal struct ProtoLabel
    {
        public readonly string Name;

        public readonly int Value;

        public ProtoLabel(int value, string name)
        {
            Value = value;
            Name = name;
        }
    }
}
