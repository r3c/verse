using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Abstract;
using Verse.DecoderDescriptors.Recurse;

namespace Verse.DecoderDescriptors
{
	class RecurseDecoderDescriptor<TEntity, TState, TValue> : AbstractDecoderDescriptor<TEntity, TState, TValue>
	{
		#region Attributes

		private readonly IDecoderConverter<TValue> converter;

		private readonly RecurseReader<TEntity, TState, TValue> reader;

		private readonly IReaderSession<TState> session;

		#endregion

		#region Constructors

		public RecurseDecoderDescriptor(IDecoderConverter<TValue> converter, IReaderSession<TState> session, RecurseReader<TEntity, TState, TValue> reader) :
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
			var descriptor = parent as RecurseDecoderDescriptor<TField, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "invalid target descriptor type");

			return this.HasField(name, assign, descriptor);
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign)
		{
			return this.HasField(name, assign, new RecurseDecoderDescriptor<TField, TState, TValue>(this.converter, this.session, this.reader.Create<TField>()));
		}

        public override IDecoderDescriptor<TEntity> HasField(string name)
        {
            var descriptor = new RecurseDecoderDescriptor<TEntity, TState, TValue>(this.converter, this.session, this.reader.Create<TEntity>());
            var recurse = descriptor.reader;

            this.reader.DeclareField(name, (ref TEntity target, TState state) => recurse.Read(ref target, state));

            return descriptor;
        }

		public override IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, IDecoderDescriptor<TElement> parent)
		{
			var descriptor = parent as RecurseDecoderDescriptor<TElement, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

			return this.IsArray(assign, descriptor);
		}

		public override IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign)
		{
			return this.IsArray(assign, new RecurseDecoderDescriptor<TElement, TState, TValue>(this.converter, this.session, this.reader.Create<TElement>()));
		}

		public override void IsValue()
		{
			this.reader.DeclareValue(this.GetConverter());
		}

		#endregion

		#region Methods / Private

		private IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, RecurseDecoderDescriptor<TField, TState, TValue> descriptor)
		{
			var constructor = this.GetConstructor<TField>();
			var recurse = descriptor.reader;

			this.reader.DeclareField(name, (ref TEntity target, TState state) =>
			{
				TField field = constructor();

				if (!recurse.Read(ref field, state))
					return false;

				assign(ref target, field);

				return true;
			});

			return descriptor;
		}

		private IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, RecurseDecoderDescriptor<TElement, TState, TValue> descriptor)
		{
			var constructor = this.GetConstructor<TElement>();
			var child = descriptor.reader;

			this.reader.DeclareArray((ref TEntity entity, TState state) =>
			{
				using (var browser = new Browser<TElement>(child.Browse(constructor, state)))
				{
					// FIXME:
					// This forces a unnecessary copy, and introduces some
					// unexpected behavior (e.g. not enumerating the sequence
					// resulting in an empty array). Method IsArray could just
					// pass enumerable elements and let parent field assignment
					// handle the copy if needed.
					assign(ref entity, browser);

					return browser.Finish();
				}
			});

			return descriptor;
		}

		#endregion
	}
}