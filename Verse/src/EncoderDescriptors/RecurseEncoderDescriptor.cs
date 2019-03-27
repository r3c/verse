using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Abstract;
using Verse.EncoderDescriptors.Recurse;

namespace Verse.EncoderDescriptors
{
	class RecurseEncoderDescriptor<TEntity, TState, TValue> : AbstractEncoderDescriptor<TEntity, TValue>
	{
		private readonly IWriterSession<TState> session;

		private readonly RecurseWriter<TEntity, TState, TValue> writer;

		public RecurseEncoderDescriptor(IEncoderConverter<TValue> converter, IWriterSession<TState> session, RecurseWriter<TEntity, TState, TValue> writer) :
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
			var descriptor = parent as RecurseEncoderDescriptor<TField, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException(nameof(parent), "incompatible descriptor type");

			return this.HasField(name, access, descriptor);
		}

		public override IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access)
		{
			return this.HasField(name, access, new RecurseEncoderDescriptor<TField, TState, TValue>(this.converter, this.session, this.writer.Create<TField>()));
		}

		public override IEncoderDescriptor<TItem> HasItems<TItem>(Func<TEntity, IEnumerable<TItem>> access, IEncoderDescriptor<TItem> parent)
		{
			var descriptor = parent as RecurseEncoderDescriptor<TItem, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException(nameof(parent), "incompatible descriptor type");

			return this.IsArray(access, descriptor);
		}

		public override IEncoderDescriptor<TItem> HasItems<TItem>(Func<TEntity, IEnumerable<TItem>> access)
		{
			return this.IsArray(access, new RecurseEncoderDescriptor<TItem, TState, TValue>(this.converter, this.session, this.writer.Create<TItem>()));
		}

		public override void IsValue()
		{
			this.writer.DeclareValue(this.GetConverter());
		}

		private RecurseEncoderDescriptor<TField, TState, TValue> HasField<TField>(string name, Func<TEntity, TField> access, RecurseEncoderDescriptor<TField, TState, TValue> descriptor)
		{
			var child = descriptor.writer;

			this.writer.DeclareField(name, (source, state) => child.WriteEntity(access(source), state));

			return descriptor;
		}

		private RecurseEncoderDescriptor<TItem, TState, TValue> IsArray<TItem>(Func<TEntity, IEnumerable<TItem>> access, RecurseEncoderDescriptor<TItem, TState, TValue> descriptor)
		{
			var child = descriptor.writer;

			this.writer.DeclareArray((source, state) => child.WriteElements(access(source), state));

			return descriptor;
		}
	}
}