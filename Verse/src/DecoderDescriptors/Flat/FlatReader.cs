using System;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.DecoderDescriptors.Flat
{
	abstract class FlatReader<TEntity, TState, TValue> : IReader<TEntity, TState>
	{
		public Converter<TValue, TEntity> converter = null;

		public EntityTree<TEntity, TState> fields = new EntityTree<TEntity, TState>();

		public abstract FlatReader<TOther, TState, TValue> Create<TOther>();

		public void DeclareField(string name, EntityReader<TEntity, TState> enter)
		{
			if (!this.fields.Connect(name, enter))
				throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");
		}
	
		public void DeclareValue(Converter<TValue, TEntity> convert)
		{
			if (this.converter != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.converter = convert;
		}

		public abstract bool ReadEntity(Func<TEntity> constructor, TState state, out TEntity entity);

		public abstract bool ReadValue(TState state, out TEntity target);
	}
}
