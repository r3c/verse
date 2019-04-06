using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;
using Verse.EncoderDescriptors.Tree;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf
{
    class Writer<TEntity> : TreeWriter<TEntity, WriterState, ProtobufValue>
    {
        private readonly ProtoBinding[] fields;

        public Writer(ProtoBinding[] fields)
        {
            this.fields = fields;
        }

		public override TreeWriter<TOther, WriterState, ProtobufValue> Create<TOther>()
        {
            return new Writer<TOther>(this.fields);
        }

		public override void DeclareField(string name, EntityWriter<TEntity, WriterState> enter)
		{
			throw new NotImplementedException();
		}

		public override void WriteElements(IEnumerable<TEntity> elements, WriterState state)
		{
			throw new NotImplementedException();
		}

		public override void WriteEntity(TEntity source, WriterState state)
		{
			throw new NotImplementedException();
		}

        protected bool TryLookup<TOther>(string name, out int index, out IWriter<TOther, WriterState> writer)
        {
            index = Array.FindIndex(this.fields, binding => binding.Name == name);

            if (index < 0)
            {
                writer = null;

                return false;
            }

            writer = new Writer<TOther>(this.fields[index].Fields);

            return true;
        }
    }
}
