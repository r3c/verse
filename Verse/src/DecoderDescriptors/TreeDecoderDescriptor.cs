using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Tree;

namespace Verse.DecoderDescriptors
{
	internal class TreeDecoderDescriptor<TState, TNative, TKey, TEntity> : IDecoderDescriptor<TNative, TEntity>
	{
		private readonly IReaderDefinition<TState, TNative, TKey, TEntity> definition;

		public TreeDecoderDescriptor(IReaderDefinition<TState, TNative, TKey, TEntity> definition)
		{
			this.definition = definition;
		}

		public IDecoder<TEntity> CreateDecoder(IReader<TState, TNative, TKey> reader)
		{
			return new TreeDecoder<TState, TNative, TKey, TEntity>(reader, this.definition.Callback);
		}


		public IDecoderDescriptor<TNative, TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter,
			IDecoderDescriptor<TNative, TElement> descriptor)
		{
			if (!(descriptor is TreeDecoderDescriptor<TState, TNative, TKey, TElement> parentDescriptor))
				throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

			return TreeDecoderDescriptor<TState, TNative, TKey, TEntity>.BindArray(this.definition, converter,
				parentDescriptor);
		}

		public IDecoderDescriptor<TNative, TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter)
		{
			var elementDefinition = this.definition.Create<TElement>();
			var elementDescriptor =
				new TreeDecoderDescriptor<TState, TNative, TKey, TElement>(elementDefinition);

			return TreeDecoderDescriptor<TState, TNative, TKey, TEntity>.BindArray(this.definition, converter,
				elementDescriptor);
		}

		public IDecoderObjectDescriptor<TNative, TObject> IsObject<TObject>(Func<TObject> constructor,
			Func<TObject, TEntity> converter)
		{
			var objectDefinition = this.definition.Create<TObject>();
			var objectLookup = objectDefinition.Lookup;
			var objectDescriptor =
				new TreeDecoderDescriptor<TState, TNative, TKey, TObject>.ObjectDescriptor(objectDefinition);

			this.definition.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity target) =>
			{
				var entity = constructor();

				switch (reader.ReadToObject(state, objectLookup.Root, ref entity))
				{
					case ReaderStatus.Succeeded:
						target = converter(entity);

						return ReaderStatus.Succeeded;

					case ReaderStatus.Ignored:
						return ReaderStatus.Ignored;

					default:
						return ReaderStatus.Failed;
				}
			};

			return objectDescriptor;
		}

		public IDecoderObjectDescriptor<TNative, TEntity> IsObject(Func<TEntity> constructor)
		{
			return this.IsObject(constructor, e => e);
		}

		public void IsValue(Setter<TEntity, TNative> converter)
		{
			this.definition.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
			{
				switch (reader.ReadToValue(state, out var value))
				{
					case ReaderStatus.Succeeded:
						// FIXME: support conversion failures
						converter(ref entity, value);

						return ReaderStatus.Succeeded;

					case ReaderStatus.Ignored:
						return ReaderStatus.Ignored;

					default:
						entity = default;

						return ReaderStatus.Failed;
				}
			};
		}

		private static TreeDecoderDescriptor<TState, TNative, TKey, TElement> BindArray<TElement>(
			IReaderDefinition<TState, TNative, TKey, TEntity> parentDefinition,
			Func<IEnumerable<TElement>, TEntity> converter,
			TreeDecoderDescriptor<TState, TNative, TKey, TElement> elementDescriptor)
		{
			var elementDefinition = elementDescriptor.definition;

			parentDefinition.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
			{
				var status = reader.ReadToArray(state, elementDefinition.Callback, out var browserMove);

				if (status != ReaderStatus.Succeeded)
					return status;

				using (var browser = new Browser<TElement>(browserMove))
				{
					entity = converter(browser);

					return browser.Finish() ? ReaderStatus.Succeeded : ReaderStatus.Failed;
				}
			};

			return elementDescriptor;
		}

		private class ObjectDescriptor : IDecoderObjectDescriptor<TNative, TEntity>
		{
			private readonly IReaderDefinition<TState, TNative, TKey, TEntity> definition;

			public ObjectDescriptor(IReaderDefinition<TState, TNative, TKey, TEntity> definition)
			{
				this.definition = definition;
			}

			public IDecoderDescriptor<TNative, TField> HasField<TField>(string name, Setter<TEntity, TField> setter,
				IDecoderDescriptor<TNative, TField> descriptor)
			{
				if (!(descriptor is TreeDecoderDescriptor<TState, TNative, TKey, TField> ancestor))
					throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

				return ObjectDescriptor.BindField(this.definition, name, setter, ancestor);
			}

			public IDecoderDescriptor<TNative, TField> HasField<TField>(string name, Setter<TEntity, TField> setter)
			{
				var fieldDefinition = this.definition.Create<TField>();
				var fieldDescriptor = new TreeDecoderDescriptor<TState, TNative, TKey, TField>(fieldDefinition);

				return ObjectDescriptor.BindField(this.definition, name, setter, fieldDescriptor);
			}

			public IDecoderDescriptor<TNative, TEntity> HasField(string name)
			{
				var fieldDefinition = this.definition.Create<TEntity>();
				var fieldDescriptor = new TreeDecoderDescriptor<TState, TNative, TKey, TEntity>(fieldDefinition);
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

			private static TreeDecoderDescriptor<TState, TNative, TKey, TField> BindField<TField>(
				IReaderDefinition<TState, TNative, TKey, TEntity> parentDefinition, string name,
				Setter<TEntity, TField> setter, TreeDecoderDescriptor<TState, TNative, TKey, TField> fieldDescriptor)
			{
				var fieldDefinition = fieldDescriptor.definition;
				var parentLookup = parentDefinition.Lookup;
				var parentRoot = parentLookup.Root;

				var success = parentLookup.ConnectTo(name,
					(IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
					{
						TField field = default;

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
}
