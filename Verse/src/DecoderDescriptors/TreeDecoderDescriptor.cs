using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Tree;

namespace Verse.DecoderDescriptors
{
	internal class TreeDecoderDescriptor<TState, TNative, TKey, TEntity> : IDecoderDescriptor<TEntity>
	{
		private readonly IDecoderConverter<TNative> converter;

		private readonly IReaderDefinition<TState, TNative, TKey, TEntity> definition;

		public TreeDecoderDescriptor(IDecoderConverter<TNative> converter, IReaderDefinition<TState, TNative, TKey, TEntity> definition)
		{
			this.converter = converter;
			this.definition = definition;
		}

		public IDecoder<TEntity> CreateDecoder(IReader<TState, TNative, TKey> reader, Func<TEntity> constructor)
		{
			return new TreeDecoder<TState, TNative, TKey, TEntity>(reader, constructor, this.definition.Callback);
		}

		public IDecoderDescriptor<TField> HasField<TField>(string name, Func<TField> constructor,
			Setter<TEntity, TField> setter, IDecoderDescriptor<TField> descriptor)
		{
			if (!(descriptor is TreeDecoderDescriptor<TState, TNative, TKey, TField> ancestor))
				throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

			return TreeDecoderDescriptor<TState, TNative, TKey, TEntity>.BindField(this.definition, name, constructor,
				setter, ancestor);
		}

		public IDecoderDescriptor<TField> HasField<TField>(string name, Func<TField> constructor,
			Setter<TEntity, TField> setter)
		{
			var fieldDefinition = this.definition.Create<TField>();
			var fieldDescriptor = new TreeDecoderDescriptor<TState, TNative, TKey, TField>(this.converter, fieldDefinition);

			return TreeDecoderDescriptor<TState, TNative, TKey, TEntity>.BindField(this.definition, name, constructor,
				setter, fieldDescriptor);
		}

		public IDecoderDescriptor<TEntity> HasField(string name)
		{
			var fieldDefinition = this.definition.Create<TEntity>();
			var fieldDescriptor = new TreeDecoderDescriptor<TState, TNative, TKey, TEntity>(this.converter, fieldDefinition);
			var parentLookup = this.definition.Lookup;
			var parentRoot = parentLookup.Root;

			var success = parentLookup.ConnectTo(name,
				(IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
					fieldDefinition.Callback(reader, state, ref entity));

			if (!success)
				throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

			this.definition.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity target) =>
				reader.ReadToObject(state, parentRoot, ref target);

			return fieldDescriptor;
		}

		public IDecoderDescriptor<TElement> HasElements<TElement>(Func<TElement> constructor,
			Setter<TEntity, IEnumerable<TElement>> setter, IDecoderDescriptor<TElement> descriptor)
		{
			if (!(descriptor is TreeDecoderDescriptor<TState, TNative, TKey, TElement> ancestor))
				throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

			return TreeDecoderDescriptor<TState, TNative, TKey, TEntity>.BindArray(this.definition, constructor, setter,
				ancestor);
		}

		public IDecoderDescriptor<TElement> HasElements<TElement>(Func<TElement> constructor,
			Setter<TEntity, IEnumerable<TElement>> setter)
		{
			var elementDefinition = this.definition.Create<TElement>();
			var elementDescriptor =
				new TreeDecoderDescriptor<TState, TNative, TKey, TElement>(this.converter, elementDefinition);

			return TreeDecoderDescriptor<TState, TNative, TKey, TEntity>.BindArray(this.definition, constructor, setter,
				elementDescriptor);
		}

		public void HasValue<TValue>(Setter<TEntity, TValue> setter)
		{
			var native = this.converter.Get<TValue>();

			this.definition.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
			{
				var readStatus = reader.ReadToValue(state, out var value);
				if (readStatus == ReaderStatus.Failed)
				{
					entity = default;
					return ReaderStatus.Failed;
				}
				else if (readStatus == ReaderStatus.Ignored)
				{
					return ReaderStatus.Ignored;
				}

				// FIXME: support conversion failures
				setter(ref entity, native(value));

				return ReaderStatus.Succeeded;
			};
		}

		public void HasValue()
		{
			var converter = this.converter.Get<TEntity>();

			// FIXME: close duplicate of previous method
			this.definition.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
			{
				var readStatus = reader.ReadToValue(state, out var value);
				if (readStatus == ReaderStatus.Failed)
				{
					entity = default;
					return ReaderStatus.Failed;
				}
				else if (readStatus == ReaderStatus.Ignored)
				{
					return ReaderStatus.Ignored;
				}

				// FIXME: support conversion failures
				entity = converter(value);

				return ReaderStatus.Succeeded;
			};
		}

		private static TreeDecoderDescriptor<TState, TNative, TKey, TElement> BindArray<TElement>(
			IReaderDefinition<TState, TNative, TKey, TEntity> parentDefinition, Func<TElement> constructor,
			Setter<TEntity, IEnumerable<TElement>> setter,
			TreeDecoderDescriptor<TState, TNative, TKey, TElement> elementDescriptor)
		{
			var elementDefinition = elementDescriptor.definition;

			parentDefinition.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
			{
				var status = reader.ReadToArray(state, constructor, elementDefinition.Callback, out var browserMove);

				if (status != ReaderStatus.Succeeded)
					return status;

				using (var browser = new Browser<TElement>(browserMove))
				{
					setter(ref entity, browser);

					return browser.Finish() ? ReaderStatus.Succeeded : ReaderStatus.Failed;
				}
			};

			return elementDescriptor;
		}

		private static TreeDecoderDescriptor<TState, TNative, TKey, TField> BindField<TField>(
			IReaderDefinition<TState, TNative, TKey, TEntity> parentDefinition, string name, Func<TField> constructor,
			Setter<TEntity, TField> setter, TreeDecoderDescriptor<TState, TNative, TKey, TField> fieldDescriptor)
		{
			var fieldDefinition = fieldDescriptor.definition;
			var parentLookup = parentDefinition.Lookup;
			var parentRoot = parentLookup.Root;

			var success = parentLookup.ConnectTo(name,
				(IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
				{
					var field = constructor();

					switch (fieldDefinition.Callback(reader, state, ref field))
					{
						case ReaderStatus.Failed:
							return ReaderStatus.Failed;

						case ReaderStatus.Succeeded:
							setter(ref entity, field);

							break;
					}

					return ReaderStatus.Succeeded;
				});

			if (!success)
				throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

			parentDefinition.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity target) =>
				reader.ReadToObject(state, parentRoot, ref target);

			return fieldDescriptor;
		}
	}
}
