using System;
using System.Collections.Generic;

namespace Verse.EncoderDescriptors.Recurse.RecurseWriters
{
	abstract class LookupRecurseWriter<TEntity, TState, TValue, TIndex> : RecurseWriter<TEntity, TState, TValue>
	{
		public readonly Dictionary<TIndex, WriteEntity<TEntity, TState>> Fields = new Dictionary<TIndex, WriteEntity<TEntity, TState>>();

		protected abstract bool TryLookup(string name, out TIndex index);

		public override void DeclareField(string name, WriteEntity<TEntity, TState> enter)
		{
			TIndex index;

			if (!this.TryLookup(name, out index))
				throw new InvalidOperationException("can't declare unknown field '" + name + "'");

			if (this.Fields.ContainsKey(index))
				throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

			this.Fields[index] = enter;
		}
	}
}
