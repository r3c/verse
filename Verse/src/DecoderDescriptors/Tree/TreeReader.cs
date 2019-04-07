using System;
using Verse.DecoderDescriptors.Base;

namespace Verse.DecoderDescriptors.Tree
{
	abstract class TreeReader<TState, TEntity, TValue> : IReader<TState, TEntity>
	{
		protected bool IsArray => this.arrayReader != null;

		protected bool IsValue => this.valueConverter != null;

		private ArrayReader<TState, TEntity> arrayReader = null;

		private Converter<TValue, TEntity> valueConverter = null;

		public abstract TreeReader<TState, TOther, TValue> Create<TOther>();

		public abstract TreeReader<TState, TField, TValue> HasField<TField>(string name, EntityReader<TState, TEntity> enter);

		public abstract bool Read(TState state, Func<TEntity> constructor, out TEntity entity);

		public abstract BrowserMove<TEntity> ReadItems(Func<TEntity> constructor, TState state);

		public void DeclareArray(ArrayReader<TState, TEntity> reader)
		{
			if (this.arrayReader != null)
				throw new InvalidOperationException("can't declare array twice on same descriptor");

			this.arrayReader = reader;
		}

		public void DeclareValue(Converter<TValue, TEntity> convert)
		{
			if (this.valueConverter != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.valueConverter = convert;
		}

		protected bool ReadArray(TState state, Func<TEntity> constructor, out TEntity entity)
		{
			return this.arrayReader(state, constructor, out entity);
		}

		protected bool ReadValue(TValue value, out TEntity entity)
		{
			entity = this.valueConverter(value);

			// FIXME: API should be updated for "value to entity" conversion
			// also be able to raise failures
			return true;
		}
	}
}