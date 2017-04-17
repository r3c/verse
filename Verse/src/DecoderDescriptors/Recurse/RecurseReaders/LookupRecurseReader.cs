using System;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Recurse.RecurseReaders
{
	abstract class LookupRecurseReader<TEntity, TState, TValue, TIndex> : RecurseReader<TEntity, TState, TValue>
	{
		private readonly Dictionary<TIndex, ReadEntity<TEntity, TState>> fields = new Dictionary<TIndex, ReadEntity<TEntity, TState>>();

		protected abstract bool TryLookup(string name, out TIndex index);

		public override void DeclareField(string name, ReadEntity<TEntity, TState> enter)
		{
			TIndex index;

			if (!this.TryLookup(name, out index))
				throw new InvalidOperationException("can't declare unknown field '" + name + "'");

			if (this.fields.ContainsKey(index))
				throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

			this.fields[index] = enter;
		}
	}
}
