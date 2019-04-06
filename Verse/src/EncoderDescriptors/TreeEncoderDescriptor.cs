using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;
using Verse.EncoderDescriptors.Tree;

namespace Verse.EncoderDescriptors
{
	class TreeEncoderDescriptor<TEntity, TState, TValue> : BaseEncoderDescriptor<TEntity, TValue>
	{
		private readonly IWriterSession<TState> session;

		private readonly TreeWriter<TState, TEntity, TValue> writer;

		public TreeEncoderDescriptor(IEncoderConverter<TValue> converter, IWriterSession<TState> session, TreeWriter<TState, TEntity, TValue> writer) :
			base(converter)
		{
			this.session = session;
			this.writer = writer;
		}

		public IEncoder<TEntity> CreateEncoder()
		{
			return new Encoder<TEntity, TState>(this.session, this.writer);
		}

		public override IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IEncoderDescriptor<TField> parent)
		{
			if (!(parent is TreeEncoderDescriptor<TField, TState, TValue> descriptor))
				throw new ArgumentOutOfRangeException(nameof(parent), "incompatible descriptor type");

			return this.HasField(name, access, descriptor);
		}

		public override IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access)
		{
			return this.HasField(name, access, new TreeEncoderDescriptor<TField, TState, TValue>(this.converter, this.session, this.writer.Create<TField>()));
		}

		public override IEncoderDescriptor<TItem> HasItems<TItem>(Func<TEntity, IEnumerable<TItem>> access, IEncoderDescriptor<TItem> parent)
		{
			if (!(parent is TreeEncoderDescriptor<TItem, TState, TValue> descriptor))
				throw new ArgumentOutOfRangeException(nameof(parent), "incompatible descriptor type");

			return this.IsArray(access, descriptor);
		}

		public override IEncoderDescriptor<TItem> HasItems<TItem>(Func<TEntity, IEnumerable<TItem>> access)
		{
			return this.IsArray(access, new TreeEncoderDescriptor<TItem, TState, TValue>(this.converter, this.session, this.writer.Create<TItem>()));
		}

		public override void IsValue()
		{
			this.writer.DeclareValue(this.GetConverter());
		}

		private TreeEncoderDescriptor<TField, TState, TValue> HasField<TField>(string name, Func<TEntity, TField> access, TreeEncoderDescriptor<TField, TState, TValue> descriptor)
		{
			var child = descriptor.writer;

			this.writer.DeclareField(name, (state, source) => child.Write(state, access(source)));

			return descriptor;
		}

		private TreeEncoderDescriptor<TItem, TState, TValue> IsArray<TItem>(Func<TEntity, IEnumerable<TItem>> access, TreeEncoderDescriptor<TItem, TState, TValue> descriptor)
		{
			var child = descriptor.writer;

			this.writer.DeclareArray((state, source) => child.WriteElements(state, access(source)));

			return descriptor;
		}
	}
}