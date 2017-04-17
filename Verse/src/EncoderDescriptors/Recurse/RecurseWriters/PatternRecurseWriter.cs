using System;
using System.Collections.Generic;

namespace Verse.EncoderDescriptors.Recurse.RecurseWriters
{
	abstract class PatternRecurseWriter<TEntity, TState, TValue> : RecurseWriter<TEntity, TState, TValue>
	{
		public Dictionary<string, WriteEntity<TEntity, TState>> Fields = new Dictionary<string, WriteEntity<TEntity, TState>>();

		public override void DeclareField(string name, WriteEntity<TEntity, TState> enter)
		{
			if (this.Fields.ContainsKey(name))
				throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

			this.Fields[name] = enter;
		}
	}
}
