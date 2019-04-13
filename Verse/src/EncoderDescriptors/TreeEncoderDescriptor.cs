using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;

namespace Verse.EncoderDescriptors
{
	class TreeEncoderDescriptor<TEntity, TState, TNative> : IEncoderDescriptor<TEntity>
	{
		private readonly IEncoderConverter<TNative> converter;

		private readonly WriterDefinition<TState, TNative, TEntity> definition;

		public TreeEncoderDescriptor(IEncoderConverter<TNative> converter, WriterDefinition<TState, TNative, TEntity> definition)
		{
			this.converter = converter;
			this.definition = definition;
		}

		public IEncoder<TEntity> CreateEncoder(IWriterSession<TState, TNative> session)
		{
			return new TreeEncoder<TState, TNative, TEntity>(session, this.definition.Callback);
		}

		public IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> getter, IEncoderDescriptor<TElement> descriptor)
		{
			if (!(descriptor is TreeEncoderDescriptor<TElement, TState, TNative> ancestor))
				throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

			var definition = ancestor.definition;

			this.definition.Callback = (session, state, entity) => session.WriteArray(state, getter(entity), definition.Callback);

			return ancestor;
		}

		public IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> getter)
		{
			var definition = this.definition.Create<TElement>();
			var descriptor = new TreeEncoderDescriptor<TElement, TState, TNative>(this.converter, definition);

			return this.IsArray(getter, descriptor);
		}

		public IEncoderObjectDescriptor<TObject> IsObject<TObject>(Func<TEntity, TObject> getter)
		{
			var definition = this.definition.Create<TObject>();
			var fields = new Dictionary<string, WriterCallback<TState, TNative, TObject>>();

			this.definition.Callback = (session, state, entity) => session.WriteObject(state, getter(entity), fields);

			return new TreeEncoderDescriptor<TObject, TState, TNative>.ObjectDescriptor<TObject>(this.converter, definition, fields);
		}

		public IEncoderObjectDescriptor<TEntity> IsObject()
		{
			return this.IsObject(self => self);
		}

		public void IsValue<TValue>(Func<TEntity, TValue> converter)
		{
			var native = this.converter.Get<TValue>();

			this.definition.Callback = (session, state, entity) => session.WriteValue(state, native(converter(entity)));
		}

		public void IsValue()
		{
			var converter = this.converter.Get<TEntity>();

			this.definition.Callback = (session, state, entity) => session.WriteValue(state, converter(entity));
		}

		private class ObjectDescriptor<TObject> : IEncoderObjectDescriptor<TObject>
		{
			private readonly WriterDefinition<TState, TNative, TObject> definition;
			private readonly IEncoderConverter<TNative> converter;
			private readonly Dictionary<string, WriterCallback<TState, TNative, TObject>> fields;

			public ObjectDescriptor(IEncoderConverter<TNative> converter, WriterDefinition<TState, TNative, TObject> definition, Dictionary<string, WriterCallback<TState, TNative, TObject>> fields)
			{
				this.converter = converter;
				this.definition = definition;
				this.fields = fields;
			}

			public IEncoderDescriptor<TField> HasField<TField>(string name, Func<TObject, TField> getter, IEncoderDescriptor<TField> descriptor)
			{
				if (!(descriptor is TreeEncoderDescriptor<TField, TState, TNative> ancestor))
					throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

				return this.HasField(name, getter, ancestor);
			}

			public IEncoderDescriptor<TField> HasField<TField>(string name, Func<TObject, TField> getter)
			{
				var definition = this.definition.Create<TField>();
				var descriptor = new TreeEncoderDescriptor<TField, TState, TNative>(this.converter, definition);

				return this.HasField(name, getter, descriptor);
			}

			private IEncoderDescriptor<TField> HasField<TField>(string name, Func<TObject, TField> getter, TreeEncoderDescriptor<TField, TState, TNative> descriptor)
			{
				if (this.fields.ContainsKey(name))
					throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

				var definition = descriptor.definition;

				this.fields[name] = (session, state, entity) => definition.Callback(session, state, getter(entity));

				return descriptor;
			}
		}
	}
}