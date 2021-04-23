using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;

namespace Verse.EncoderDescriptors
{
	internal class TreeEncoderDescriptor<TState, TNative, TEntity> : IEncoderDescriptor<TEntity>
	{
		private readonly IEncoderConverter<TNative> converter;

		private readonly IWriterDefinition<TState, TNative, TEntity> definition;

		private readonly Dictionary<string, WriterCallback<TState, TNative, TEntity>> fields;

		public TreeEncoderDescriptor(IEncoderConverter<TNative> converter, IWriterDefinition<TState, TNative, TEntity> definition)
		{
			this.converter = converter;
			this.definition = definition;
			this.fields = new Dictionary<string, WriterCallback<TState, TNative, TEntity>>();
		}

		public IEncoder<TEntity> CreateEncoder(IWriter<TState, TNative> reader)
		{
			return new TreeEncoder<TState, TNative, TEntity>(reader, this.definition.Callback);
		}

		public IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> getter,
			IEncoderDescriptor<TField> descriptor)
		{
			if (!(descriptor is TreeEncoderDescriptor<TState, TNative, TField> ancestor))
				throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

			return TreeEncoderDescriptor<TState, TNative, TEntity>.BindField(this.definition, name, this.fields, getter,
				ancestor);
		}

		public IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> getter)
		{
			var fieldDefinition = this.definition.Create<TField>();
			var fieldDescriptor = new TreeEncoderDescriptor<TState, TNative, TField>(this.converter, fieldDefinition);

			return TreeEncoderDescriptor<TState, TNative, TEntity>.BindField(this.definition, name, this.fields, getter,
				fieldDescriptor);
		}

		public IEncoderDescriptor<TEntity> HasField(string name)
		{
			return this.HasField(name, e => e);
		}

		public IEncoderDescriptor<TElement> HasElements<TElement>(Func<TEntity, IEnumerable<TElement>> getter,
			IEncoderDescriptor<TElement> descriptor)
		{
			if (!(descriptor is TreeEncoderDescriptor<TState, TNative, TElement> ancestor))
				throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

			return TreeEncoderDescriptor<TState, TNative, TEntity>.BindArray(this.definition, getter, ancestor);
		}

		public IEncoderDescriptor<TElement> HasElements<TElement>(Func<TEntity, IEnumerable<TElement>> getter)
		{
			var elementDefinition = this.definition.Create<TElement>();
			var elementDescriptor =
				new TreeEncoderDescriptor<TState, TNative, TElement>(this.converter, elementDefinition);

			return TreeEncoderDescriptor<TState, TNative, TEntity>.BindArray(this.definition, getter, elementDescriptor);
		}

		public void HasValue<TValue>(Func<TEntity, TValue> converter)
		{
			var native = this.converter.Get<TValue>();

			TreeEncoderDescriptor<TState, TNative, TEntity>.BindValue(this.definition, e => native(converter(e)));

			this.definition.Callback = (reader, state, entity) => reader.WriteAsValue(state, native(converter(entity)));
		}

		public void HasValue()
		{
			var converter = this.converter.Get<TEntity>();

			TreeEncoderDescriptor<TState, TNative, TEntity>.BindValue(this.definition, converter);
		}

		public void HasRawContent<TValue>(Func<TEntity, TValue> getter)
		{
			var native = this.converter.Get<TValue>();

			this.definition.Callback = (writer, state, entity) => writer.WriteAsRawValue(state, native(getter(entity)));
		}

		private static IEncoderDescriptor<TElement> BindArray<TElement>(
			IWriterDefinition<TState, TNative, TEntity> parent, Func<TEntity, IEnumerable<TElement>> getter,
			TreeEncoderDescriptor<TState, TNative, TElement> elementDescriptor)
		{
			var elementDefinition = elementDescriptor.definition;

			parent.Callback = (reader, state, entity) =>
				reader.WriteAsArray(state, getter(entity), elementDefinition.Callback);

			return elementDescriptor;
		}

		private static IEncoderDescriptor<TField> BindField<TField>(
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

		private static void BindValue(IWriterDefinition<TState, TNative, TEntity> parent,
			Converter<TEntity, TNative> converter)
		{
			parent.Callback = (reader, state, entity) => reader.WriteAsValue(state, converter(entity));
		}
	}
}
