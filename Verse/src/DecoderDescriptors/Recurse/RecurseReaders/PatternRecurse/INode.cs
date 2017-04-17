using System;

namespace Verse.DecoderDescriptors.Recurse.RecurseReaders.PatternRecurse
{
	interface INode<TEntity, TValue, TState>
	{
		bool IsConnected
		{
			get;
		}

		#region Methods

		void Assign(ref TEntity target, TValue value);

		bool Enter(ref TEntity target, IRecurseReader<TEntity, TState, TValue> unknown, TState state);

		INode<TEntity, TValue, TState> Follow(char c);

		#endregion
	}
}