using System;
using Verse.ParserDescriptors.Recurse.Nodes;

namespace Verse.ParserDescriptors.Recurse
{
	class Container<T, C, V>
	{
		public BranchNode<T, C, V>	fields = new BranchNode<T, C, V> ();

		public Follow<T, C, V>		items = null;

		public ParserAssign<T, V>	value = null;
	}
}
