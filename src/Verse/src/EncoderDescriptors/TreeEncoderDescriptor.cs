using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;

namespace Verse.EncoderDescriptors
{
	internal class TreeEncoderDescriptor<TState, TNative, TEntity> : IEncoderDescriptor<TNative, TEntity>
	{
		private readonly IWriterDefinition<TState, TNative, TEntity> definition;

		private readonly Dictionary<string, WriterCallback<TState, TNative, TEntity>> fields;

		public TreeEncoderDescriptor(IWriterDefinition<TState, TNative, TEntity> definition)
		{
			this.definition = definition;
			fields = new Dictionary<string, WriterCallback<TState, TNative, TEntity>>();
		}

		public IEncoder<TEntity> CreateEncoder(IWriter<TState, TNative> reader)
		{
			return new TreeEncoder<TState, TNative, TEntity>(reader, definition.Callback);
		}

		public IEncoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TEntity, TField> getter,
			IEncoderDescriptor<TNative, TField> descriptor)
		{
			if (!(descriptor is TreeEncoderDescriptor<TState, TNative, TField> ancestor))
				throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

			return BindField(definition, name, fields, getter,
				ancestor);
		}

		public IEncoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TEntity, TField> getter)
		{
			var fieldDefinition = definition.Create<TField>();
			var fieldDescriptor = new TreeEncoderDescriptor<TState, TNative, TField>(fieldDefinition);

			return BindField(definition, name, fields, getter,
				fieldDescriptor);
		}

		public IEncoderDescriptor<TNative, TEntity> HasField(string name)
		{
			return HasField(name, e => e);
		}

		public IEncoderDescriptor<TNative, TElement> HasElements<TElement>(Func<TEntity, IEnumerable<TElement>> getter,
			IEncoderDescriptor<TNative, TElement> descriptor)
		{
			if (!(descriptor is TreeEncoderDescriptor<TState, TNative, TElement> ancestor))
				throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

			return BindArray(definition, getter, ancestor);
		}

		public IEncoderDescriptor<TNative, TElement> HasElements<TElement>(Func<TEntity, IEnumerable<TElement>> getter)
		{
			var elementDefinition = definition.Create<TElement>();
			var elementDescriptor = new TreeEncoderDescriptor<TState, TNative, TElement>(elementDefinition);

			return BindArray(definition, getter,
				elementDescriptor);
		}

		public void HasValue(Func<TEntity, TNative> converter)
		{
			definition.Callback = (session, state, entity) => session.WriteAsValue(state, converter(entity));
		}

		private static IEncoderDescriptor<TNative, TElement> BindArray<TElement>(
			IWriterDefinition<TState, TNative, TEntity> parent, Func<TEntity, IEnumerable<TElement>> getter,
			TreeEncoderDescriptor<TState, TNative, TElement> elementDescriptor)
		{
			var elementDefinition = elementDescriptor.definition;

			parent.Callback = (reader, state, entity) =>
				reader.WriteAsArray(state, getter(entity), elementDefinition.Callback);

			return elementDescriptor;
		}

		private static IEncoderDescriptor<TNative, TField> BindField<TField>(
			IWriterDefinition<TState, TNative, TEntity> parentDefinition,
			string name, Dictionary<string, WriterCallback<TState, TNative, TEntity>> parentFields,
			Func<TEntity, TField> getter, TreeEncoderDescriptor<TState, TNative, TField> fieldDescriptor)
		{
			if (parentFields.ContainsKey(name))
				throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

			parentDefinition.Callback = (reader, state, entity) => reader.WriteAsObject(state, entity, parentFields);

			var fieldDefinition = fieldDescriptor.definition;

			parentFields[name] = (reader, state, entity) => fieldDefinition.Callback(reader, state, getter(entity));

			return fieldDescriptor;
		}
	}
}
