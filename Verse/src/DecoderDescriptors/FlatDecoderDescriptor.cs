using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Abstract;
using Verse.DecoderDescriptors.Flat;

namespace Verse.DecoderDescriptors
{
	class FlatDecoderDescriptor<TEntity, TState, TValue> : AbstractDecoderDescriptor<TEntity, TState, TValue>
	{
		#region Attributes

		private readonly IDecoderConverter<TValue> converter;

		private readonly IFlatReader<TEntity, TState, TValue> reader;

		private readonly IReaderSession<TState> session;

		#endregion

		#region Constructors

		public FlatDecoderDescriptor(IDecoderConverter<TValue> converter, IReaderSession<TState> session, IFlatReader<TEntity, TState, TValue> reader) :
			base(converter, session, reader)
		{
			this.converter = converter;
			this.reader = reader;
			this.session = session;
		}

		#endregion

		#region Methods / Public

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent)
		{
			var descriptor = parent as FlatDecoderDescriptor<TField, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "invalid target descriptor type");

			return this.HasField(name, assign, descriptor);
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign)
		{
			return this.HasField(name, assign, new FlatDecoderDescriptor<TField, TState, TValue>(this.converter, this.session, this.reader.Create<TField>()));
		}

		public override IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, IDecoderDescriptor<TElement> parent)
		{
			throw new NotImplementedException("array is not supported");
		}

		public override IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign)
		{
			throw new NotImplementedException("array is not supported");
		}

		public override void IsValue()
		{
			this.reader.DeclareValue(this.GetConverter());
		}

		#endregion

		#region Methods / Private

		private IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, FlatDecoderDescriptor<TField, TState, TValue> descriptor)
		{
			var constructor = this.GetConstructor<TField>();
			var field = descriptor.reader;

			this.reader.DeclareField(name, (ref TEntity target, TState state) =>
			{
				TField inner;

				if (!field.ReadValue(constructor, state, out inner))
					return false;

				assign(ref target, inner);

				return true;
			});

			return descriptor;
		}

		#endregion
	}
}