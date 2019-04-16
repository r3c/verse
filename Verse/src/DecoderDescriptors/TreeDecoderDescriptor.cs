using System;
using System.Collections.Generic;
using System.Linq;
using Verse.DecoderDescriptors.Tree;
using Verse.Lookups;

namespace Verse.DecoderDescriptors
{
	class TreeDecoderDescriptor<TEntity, TState, TNative> : IDecoderDescriptor<TEntity>
	{
		private readonly IDecoderConverter<TNative> converter;

		private readonly ReaderDefinition<TState, TNative, TEntity> definition;

		public TreeDecoderDescriptor(IDecoderConverter<TNative> converter, ReaderDefinition<TState, TNative, TEntity> definition)
		{
			this.converter = converter;
			this.definition = definition;
		}

		public IDecoder<TEntity> CreateDecoder(IReaderSession<TState, TNative> session)
		{
			return new TreeDecoder<TState, TNative, TEntity>(session, this.definition.Callback);
		}

		public IDecoderDescriptor<TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter, IDecoderDescriptor<TElement> parent)
		{
			if (!(parent is TreeDecoderDescriptor<TElement, TState, TNative> ancestor))
				throw new ArgumentOutOfRangeException(nameof(parent), "incompatible descriptor type");

			var definition = ancestor.definition;

			this.definition.Callback = (IReaderSession<TState, TNative> session, TState state, out TEntity entity) =>
			{
				using (var browser = new Browser<TElement>(session.ReadToArray(state, definition.Callback)))
				{
					entity = converter(browser);

					return browser.Finish();
				}
			};

			return ancestor;
		}

		public IDecoderDescriptor<TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter)
		{
			var definition = this.definition.Create<TElement>();
			var descriptor = new TreeDecoderDescriptor<TElement, TState, TNative>(this.converter, definition);

			return this.IsArray(converter, descriptor);
		}

		public IDecoderObjectDescriptor<TObject> IsObject<TObject>(Func<TObject> constructor, Func<TObject, TEntity> converter)
		{
			var definition = this.definition.Create<TObject>();
			var fields = new NameLookup<ReaderSetter<TState, TNative, TObject>>();

			this.definition.Callback = (IReaderSession<TState, TNative> session, TState state, out TEntity target) =>
			{
				var entity = constructor();

				if (!session.ReadToObject(state, fields, ref entity))
				{
					target = default;

					return false;
				}

				target = converter(entity);

				return true;
			};

			return new TreeDecoderDescriptor<TObject, TState, TNative>.ObjectDescriptor<TObject>(this.converter, definition, fields);
		}

		public IDecoderObjectDescriptor<TEntity> IsObject(Func<TEntity> constructor)
		{
			return this.IsObject(constructor, self => self);
		}

		public void IsValue<TValue>(Func<TValue, TEntity> converter)
		{
			var native = this.converter.Get<TValue>();

			this.definition.Callback = (IReaderSession<TState, TNative> session, TState state, out TEntity entity) =>
			{
				if (!session.ReadToValue(state, out var value))
				{
					entity = default;

					return false;
				}

				// FIXME: support conversion failures
				entity = converter(native(value));

				return true;
			};
		}

		public void IsValue()
		{
			var converter = this.converter.Get<TEntity>();

			// FIXME: close duplicate of previous method
			this.definition.Callback = (IReaderSession<TState, TNative> session, TState state, out TEntity entity) =>
			{
				if (!session.ReadToValue(state, out var value))
				{
					entity = default;

					return false;
				}

				// FIXME: support conversion failures
				entity = converter(value);

				return true;
			};
		}

		private class ObjectDescriptor<TObject> : IDecoderObjectDescriptor<TObject>
		{
			private readonly IDecoderConverter<TNative> converter;
			private readonly ReaderDefinition<TState, TNative, TObject> definition;
			private readonly NameLookup<ReaderSetter<TState, TNative, TObject>> fields;

			public ObjectDescriptor(IDecoderConverter<TNative> converter, ReaderDefinition<TState, TNative, TObject> definition, NameLookup<ReaderSetter<TState, TNative, TObject>> fields)
			{
				this.converter = converter;
				this.definition = definition;
				this.fields = fields;
			}

			public IDecoderDescriptor<TField> HasField<TField>(string name, Setter<TObject, TField> setter, IDecoderDescriptor<TField> descriptor)
			{
				if (!(descriptor is TreeDecoderDescriptor<TField, TState, TNative> ancestor))
					throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

				return this.HasField(name, setter, ancestor);
			}

			public IDecoderDescriptor<TField> HasField<TField>(string name, Setter<TObject, TField> setter)
			{
				var definition = this.definition.Create<TField>();
				var descriptor = new TreeDecoderDescriptor<TField, TState, TNative>(this.converter, definition);

				return this.HasField(name, setter, descriptor);
			}

			private IDecoderDescriptor<TField> HasField<TField>(string name, Setter<TObject, TField> setter, TreeDecoderDescriptor<TField, TState, TNative> descriptor)
			{
				var definition = descriptor.definition;
				var success = this.fields.ConnectTo(name,
					(IReaderSession<TState, TNative> session, TState state, ref TObject entity) =>
					{
						if (!definition.Callback(session, state, out var field))
							return false;

						setter(ref entity, field);

						return true;
					});

				if (!success)
					throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

				return descriptor;
			}
		}
	}
}