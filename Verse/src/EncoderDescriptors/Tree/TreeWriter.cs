using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;

namespace Verse.EncoderDescriptors.Tree
{
	abstract class TreeWriter<TEntity, TState, TValue> : IWriter<TEntity, TState>
	{
		public bool IsArray => this.array != null;

	    public bool IsValue => this.value != null;

	    private EntityWriter<TEntity, TState> array = null;

		private Converter<TEntity, TValue> value = null;

		public abstract TreeWriter<TOther, TState, TValue> Create<TOther>();

		public abstract void DeclareField(string name, EntityWriter<TEntity, TState> enter);

		public abstract void WriteElements(IEnumerable<TEntity> elements, TState state);

		public abstract void WriteEntity(TEntity source, TState state);

		public void DeclareArray(EntityWriter<TEntity, TState> enter)
		{
			if (this.array != null)
				throw new InvalidOperationException("can't declare array twice on same descriptor");

			this.array = enter;            
		}

		public void DeclareValue(Converter<TEntity, TValue> converter)
		{
			if (this.value != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.value = converter;
		}

		protected TValue ConvertValue(TEntity entity)
		{
			return this.value(entity);
		}

		protected void WriteArray(TEntity entity, TState state)
		{
			this.array(entity, state);
		}
	}
}
