using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;

namespace Verse.EncoderDescriptors.Tree
{
	abstract class TreeWriter<TState, TEntity, TValue> : IWriter<TState, TEntity>
	{
		private EntityWriter<TState, TEntity> arrayWriter = null;

		private readonly Dictionary<string, EntityWriter<TState, TEntity>> fieldWriters = new Dictionary<string, EntityWriter<TState, TEntity>>();

		private Converter<TEntity, TValue> valueConverter = null;

		public abstract TreeWriter<TState, TOther, TValue> Create<TOther>();

		public abstract void WriteElements(TState state, IEnumerable<TEntity> elements);

		public abstract void WriteFields(TState state, TEntity source, IReadOnlyDictionary<string, EntityWriter<TState, TEntity>> fields);

		public abstract void WriteNull(TState state);

		public abstract void WriteValue(TState state, TValue value);

		public void DeclareArray(EntityWriter<TState, TEntity> writer)
		{
			if (this.arrayWriter != null)
				throw new InvalidOperationException("can't declare array twice on same descriptor");

			this.arrayWriter = writer;
		}

		public void DeclareField(string name, EntityWriter<TState, TEntity> writer)
		{
			if (this.fieldWriters.ContainsKey(name))
				throw new InvalidOperationException($"can't declare same field '{name}' twice on same descriptor");

			this.fieldWriters[name] = writer;
		}

		public void DeclareValue(Converter<TEntity, TValue> converter)
		{
			if (this.valueConverter != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.valueConverter = converter;
		}

		public void Write(TState state, TEntity entity)
		{
			if (entity == null)
				this.WriteNull(state);
			else if (this.arrayWriter != null)
				this.arrayWriter(state, entity);
			else if (this.valueConverter != null)
				this.WriteValue(state, this.valueConverter(entity));
			else
				this.WriteFields(state, entity, this.fieldWriters);
		}
	}
}
