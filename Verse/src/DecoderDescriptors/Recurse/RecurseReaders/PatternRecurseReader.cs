using System;
using Verse.DecoderDescriptors.Recurse;
using Verse.DecoderDescriptors.Recurse.RecurseReaders.PatternRecurse.Nodes;

namespace Verse.DecoderDescriptors.Recurse.RecurseReaders
{
	abstract class PatternRecurseReader<TEntity, TState, TValue> : RecurseReader<TEntity, TState, TValue>
	{
		public readonly BranchNode<TEntity, TValue, TState> RootNode = new BranchNode<TEntity, TValue, TState>();

		public override void DeclareField(string name, ReadEntity<TEntity, TState> enter)
		{
			BranchNode<TEntity, TValue, TState> next = this.RootNode;

			foreach (char character in name)
				next = next.Connect(character);

			if (next.enter != null)
				throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

			next.enter = enter;
		}
	}
}