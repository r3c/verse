using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Abstract;
using Verse.EncoderDescriptors.Recurse;

namespace Verse.EncoderDescriptors
{
	class RecurseEncoderDescriptor<TEntity, TState, TValue> : AbstractEncoderDescriptor<TEntity, TValue>
	{
		#region Attributes

		private readonly IWriterSession<TState> session;

		private readonly RecurseWriter<TEntity, TState, TValue> writer;

		#endregion

		#region Constructors

		public RecurseEncoderDescriptor(IEncoderConverter<TValue> converter, IWriterSession<TState> session, RecurseWriter<TEntity, TState, TValue> writer) :
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
			var descriptor = parent as RecurseEncoderDescriptor<TField, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

			return this.HasField(name, access, descriptor);
		}

		public override IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access)
		{
			return this.HasField(name, access, new RecurseEncoderDescriptor<TField, TState, TValue>(this.converter, this.session, this.writer.Create<TField>()));
		}

		public override IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IEncoderDescriptor<TElement> parent)
		{
			var descriptor = parent as RecurseEncoderDescriptor<TElement, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

			return this.IsArray(access, descriptor);
		}

		public override IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access)
		{
			return this.IsArray(access, new RecurseEncoderDescriptor<TElement, TState, TValue>(this.converter, this.session, this.writer.Create<TElement>()));
		}

		public override void IsValue()
		{
			this.writer.DeclareValue(this.GetConverter());
		}

		#endregion

		#region Methods / Private

		private RecurseEncoderDescriptor<TField, TState, TValue> HasField<TField>(string name, Func<TEntity, TField> access, RecurseEncoderDescriptor<TField, TState, TValue> descriptor)
		{
			var recurse = descriptor.writer;

			this.writer.DeclareField(name, (source, state) => recurse.WriteEntity(access(source), state));

			return descriptor;
		}

		private RecurseEncoderDescriptor<TElement, TState, TValue> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, RecurseEncoderDescriptor<TElement, TState, TValue> descriptor)
		{
			var recurse = descriptor.writer;

			this.writer.DeclareArray((source, state) => recurse.WriteElements(access(source), state));

			return descriptor;
		}

		#endregion
	}
}