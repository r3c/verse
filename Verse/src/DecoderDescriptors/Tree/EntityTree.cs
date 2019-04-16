using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Tree
{
	class EntityTree<TNode>
	{
		private const int INDEX_SIZE = 128;

		public static readonly EntityTree<TNode> Empty = new EntityTree<TNode>();

		public TNode Value;

		private Dictionary<char, EntityTree<TNode>> highChildren = null;

		private EntityTree<TNode>[] lowChildren = null;

		public bool Connect(string name, TNode value)
		{
			var current = this;
			var next = this;

			foreach (char character in name)
			{
				if (character < INDEX_SIZE)
				{
					if (current.lowChildren == null)
						current.lowChildren = new EntityTree<TNode>[INDEX_SIZE];

					if (current.lowChildren[character] != null)
						next = current.lowChildren[character];
					else
					{
						next = new EntityTree<TNode>();

						current.lowChildren[character] = next;
					}
				}
				else
				{
					if (current.highChildren == null)
						current.highChildren = new Dictionary<char, EntityTree<TNode>>();

					if (!current.highChildren.TryGetValue(character, out next))
					{
						next = new EntityTree<TNode>();

						current.highChildren[character] = next;
					}
				}

				current = next;
			}

			if (next.Value != null)
				return false;

			next.Value = value;

			return true;
		}

		public EntityTree<TNode> Follow(char character)
		{
			if (character < INDEX_SIZE)
			{
				if (this.lowChildren != null && this.lowChildren[character] != null)
					return this.lowChildren[character];
			}
			else
			{
				if (this.highChildren != null && this.highChildren.TryGetValue(character, out var next))
					return next;
			}

			return EntityTree<TNode>.Empty;
		}
	}
}
