using System;
using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Readers.Pattern.Nodes;

namespace Verse.ParserDescriptors.Recurse.Readers
{
    abstract class PatternReader<TEntity, TValue, TState> : AbstractReader<TEntity, TValue, TState>
    {
        public readonly BranchNode<TEntity, TValue, TState> RootNode = new BranchNode<TEntity, TValue, TState>();

        public override void DeclareField(string name, Enter<TEntity, TState> enter)
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