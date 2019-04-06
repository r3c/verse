using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Base;
using Verse.DecoderDescriptors.Flat;

namespace Verse.DecoderDescriptors
{
	class FlatDecoderDescriptor<TEntity, TState, TValue> : BaseDecoderDescriptor<TEntity, TState, TValue>
	{
		private readonly IDecoderConverter<TValue> converter;

		private readonly FlatReader<TEntity, TState, TValue> reader;

		private readonly IReaderSession<TState> session;

		public FlatDecoderDescriptor(IDecoderConverter<TValue> converter, IReaderSession<TState> session, FlatReader<TEntity, TState, TValue> reader) :
			base(converter, session, reader)
		{
			this.converter = converter;
			this.reader = reader;
			this.session = session;
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent)
		{
			var descriptor = parent as FlatDecoderDescriptor<TField, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException(nameof(parent), "invalid target descriptor type");

			return this.HasField(name, assign, descriptor);
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign)
		{
			return this.HasField(name, assign, new FlatDecoderDescriptor<TField, TState, TValue>(this.converter, this.session, this.reader.Create<TField>()));
		}

		public override IDecoderDescriptor<TEntity> HasField(string name)
		{
			var descriptor = new FlatDecoderDescriptor<TEntity, TState, TValue>(this.converter, this.session, this.reader.Create<TEntity>());
			var child = descriptor.reader;
			
			this.reader.DeclareField(name, (TState state, ref TEntity target) => child.ReadValue (state, out target));
			
			return descriptor;
		}

		public override IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign, IDecoderDescriptor<TItem> parent)
		{
			throw new NotImplementedException("array is not supported");
		}

		public override IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign)
		{
			throw new NotImplementedException("array is not supported");
		}

		public override void IsValue()
		{
			this.reader.DeclareValue(this.GetConverter());
		}

		private IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, FlatDecoderDescriptor<TField, TState, TValue> descriptor)
		{
			var child = descriptor.reader;

			this.reader.DeclareField(name, (TState state, ref TEntity target) =>
			{
			    if (!child.ReadValue(state, out var inner))
					return false;

				assign(ref target, inner);

				return true;
			});

			return descriptor;
		}
	}
}