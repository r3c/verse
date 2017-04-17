using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Abstract;
using Verse.EncoderDescriptors.Recurse;

namespace Verse.EncoderDescriptors
{
	class RecurseEncoderDescriptor<TEntity, TValue, TState> : AbstractEncoderDescriptor<TEntity, TValue>
	{
		#region Attributes

		private readonly IWriterSession<TState> session;

		private readonly IWriter<TEntity, TValue, TState> writer;

		#endregion

		#region Constructors

		public RecurseEncoderDescriptor(IEncoderConverter<TValue> converter, IWriterSession<TState> session, IWriter<TEntity, TValue, TState> writer) :
			base(converter)
		{
			this.session = session;
			this.writer = writer;
		}

		#endregion

		#region Methods / Public

		public IEncoder<TEntity> CreateEncoder()
		{
			return new Encoder<TEntity, TValue, TState>(this.session, this.writer);
		}

		public override IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IEncoderDescriptor<TField> parent)
		{
			var descriptor = parent as RecurseEncoderDescriptor<TField, TValue, TState>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

			return this.HasField(name, access, descriptor);
		}

		public override IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access)
		{
			return this.HasField(name, access, new RecurseEncoderDescriptor<TField, TValue, TState>(this.converter, this.session, this.writer.Create<TField>()));
		}

		public override IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IEncoderDescriptor<TElement> parent)
		{
			var descriptor = parent as RecurseEncoderDescriptor<TElement, TValue, TState>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

			return this.IsArray(access, descriptor);
		}

		public override IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access)
		{
			return this.IsArray(access, new RecurseEncoderDescriptor<TElement, TValue, TState>(this.converter, this.session, this.writer.Create<TElement>()));
		}

		public override void IsValue()
		{
			this.writer.DeclareValue(this.GetConverter());
		}

		#endregion

		#region Methods / Private

		private RecurseEncoderDescriptor<TField, TValue, TState> HasField<TField>(string name, Func<TEntity, TField> access, RecurseEncoderDescriptor<TField, TValue, TState> descriptor)
		{
			var recurse = descriptor.writer;

			this.writer.DeclareField(name, (source, state) => recurse.WriteEntity(access(source), state));

			return descriptor;
		}

		private RecurseEncoderDescriptor<TElement, TValue, TState> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, RecurseEncoderDescriptor<TElement, TValue, TState> descriptor)
		{
			var recurse = descriptor.writer;

			this.writer.DeclareArray((source, state) => recurse.WriteElements(access(source), state));

			return descriptor;
		}

		#endregion
	}
}