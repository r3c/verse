using System;
using Verse.DecoderDescriptors.Flat.FlatReaders.PatternFlat.Nodes;

namespace Verse.DecoderDescriptors.Flat.FlatReaders
{
	abstract class PatternFlatReader<TEntity, TState, TValue> : IFlatReader<TEntity, TState, TValue>
	{
		public BranchNode<TEntity, TState, TValue> fields = new BranchNode<TEntity, TState, TValue>();

		public Converter<TValue, TEntity> value = null;

		public abstract IFlatReader<TOther, TState, TValue> Create<TOther> ();

		public void DeclareField(string name, ReadEntity<TEntity, TState> enter)
		{
			BranchNode<TEntity, TState, TValue> next = this.fields;

			foreach (char c in name)
				next = next.Connect(c);

			if (next.enter != null)
				throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

			next.enter = enter;
		}
	
		public void DeclareValue(Converter<TValue, TEntity> convert)
		{
			if (this.value != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.value = convert;
		}

		public abstract bool ReadEntity(Func<TEntity> constructor, TState state, out TEntity entity);

		public abstract bool ReadValue(Func<TEntity> constructor, TState state, out TEntity target);
	}
}
