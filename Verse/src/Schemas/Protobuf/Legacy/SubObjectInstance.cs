using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Verse.Schemas.Protobuf.Legacy
{
	struct SubObjectInstance
	{
		public readonly int Index;

		public readonly MemoryStream Stream;

		public readonly ProtoWriter Writer;

		public SubObjectInstance(int index)
		{
			this.Index = index;
			this.Stream = new MemoryStream();
			this.Writer = new ProtoWriter(this.Stream, TypeModel.Create(), null);
		}
	}
}
