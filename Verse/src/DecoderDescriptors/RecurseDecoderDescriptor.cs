using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Abstract;
using Verse.DecoderDescriptors.Recurse;
using Verse.Tools;

namespace Verse.DecoderDescriptors
{
	class RecurseDecoderDescriptor<TEntity, TValue, TState> : AbstractDecoderDescriptor<TEntity, TValue>
	{
		#region Attributes

		private readonly IReader<TEntity, TValue, TState> reader;

		#endregion

		#region Constructors

		public RecurseDecoderDescriptor(IDecoderConverter<TValue> converter, IReader<TEntity, TValue, TState> reader) :
			base(converter)
		{
			this.reader = reader;
		}

		#endregion

		#region Methods / Public

		public IDecoder<TEntity> CreateDecoder()
		{
			return new Decoder<TEntity, TValue, TState>(this.GetConstructor<TEntity>(), this.reader);
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent)
		{
			var descriptor = parent as RecurseDecoderDescriptor<TField, TValue, TState>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "invalid target descriptor type");

			return this.HasField(name, assign, descriptor);
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign)
		{
			return this.HasField(name, assign, new RecurseDecoderDescriptor<TField, TValue, TState>(this.converter, this.reader.Create<TField>()));
		}

		public override IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, IDecoderDescriptor<TElement> parent)
		{
			var descriptor = parent as RecurseDecoderDescriptor<TElement, TValue, TState>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

			return this.IsArray(assign, descriptor);
		}

		public override IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign)
		{
			return this.IsArray(assign, new RecurseDecoderDescriptor<TElement, TValue, TState>(this.converter, this.reader.Create<TElement>()));
		}

		public override void IsValue()
		{
			this.reader.DeclareValue(this.GetConverter());
		}

		#endregion

		#region Methods / Private

		private IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, RecurseDecoderDescriptor<TField, TValue, TState> descriptor)
		{
			var constructor = this.GetConstructor<TField>();
			var recurse = descriptor.reader;

			this.reader.DeclareField(name, (ref TEntity target, TState state) =>
			{
				TField field;

				if (!recurse.ReadEntity(constructor, state, out field))
					return false;

				assign(ref target, field);

				return true;
			});

			return descriptor;
		}

		private IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, RecurseDecoderDescriptor<TElement, TValue, TState> descriptor)
		{
			var elementConstructor = this.GetConstructor<TElement>();
			var elementReader = descriptor.reader;

			this.reader.DeclareArray((Func<TEntity> entityConstructor, TState state, out TEntity entity) =>
			{
				using (var browser = new Browser<TElement>(elementReader.ReadElements(elementConstructor, state)))
				{
					// FIXME:
					// This forces a unnecessary copy, and introduces some
					// unexpected behavior (e.g. not enumerating the sequence
					// resulting in an empty array). Method IsArray could just
					// pass enumerable elements and let parent field assignment
					// handle the copy if needed.
					entity = entityConstructor();

					assign(ref entity, browser);

					return browser.Finish();
				}
			});

			return descriptor;
		}

		#endregion
	}
}