using System;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.DecoderDescriptors.Recurse
{
	abstract class RecurseReader<TEntity, TState, TValue> : IReader<TEntity, TState>
	{
		protected bool HoldValue
		{
			get
			{
				return this.convert != null;
			}
		}

		private Converter<TValue, TEntity> convert = null;

		public abstract BrowserMove<TEntity> Browse(Func<TEntity> constructor, TState state);

		public abstract RecurseReader<TField, TState, TValue> HasField<TField>(string name, EntityReader<TEntity, TState> enter);

		public abstract RecurseReader<TItem, TState, TValue> HasItems<TItem>(EntityReader<TEntity, TState> enter);

		public abstract bool Read(ref TEntity entity, TState state);

		public void IsValue(Converter<TValue, TEntity> convert)
		{
			if (this.convert != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.convert = convert;
		}

		protected TEntity ConvertValue(TValue value)
		{
			return this.convert(value);
		}
	}
}