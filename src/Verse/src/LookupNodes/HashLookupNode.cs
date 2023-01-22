using System;
using System.Collections.Generic;

namespace Verse.LookupNodes
{
	internal static class HashLookupNode
	{
		public const int HashThreshold = 128;
	}

	internal class HashLookupNode<TKey, TValue> : ILookupNode<TKey, TValue>
	{
		public bool HasValue { get; set; }
		public TValue Value { get; set; }

		private static readonly ILookupNode<TKey, TValue> Empty = new HashLookupNode<TKey, TValue>(k => default);

		private readonly Func<TKey, int> extractor;

		private Dictionary<int, HashLookupNode<TKey, TValue>> hashedChildren;

		private HashLookupNode<TKey, TValue>[] indexedChildren;

		public HashLookupNode(Func<TKey, int> extractor)
		{
			this.extractor = extractor;
		}

		public HashLookupNode<TKey, TValue> ConnectTo(TKey key)
		{
			var character = extractor(key);

			if (character < HashLookupNode.HashThreshold)
			{
				if (indexedChildren == null)
					indexedChildren = new HashLookupNode<TKey, TValue>[HashLookupNode.HashThreshold];

				if (indexedChildren[character] != null)
					return indexedChildren[character];

				var next = new HashLookupNode<TKey, TValue>(extractor);

				indexedChildren[character] = next;

				return next;
			}
			else
			{
				if (hashedChildren == null)
					hashedChildren = new Dictionary<int, HashLookupNode<TKey, TValue>>();

				if (hashedChildren.TryGetValue(character, out var next))
					return next;

				next = new HashLookupNode<TKey, TValue>(extractor);

				hashedChildren[character] = next;

				return next;
			}
		}

		public ILookupNode<TKey, TValue> Follow(TKey key)
		{
			var character = extractor(key);

			if (character < HashLookupNode.HashThreshold)
			{
				if (indexedChildren != null && indexedChildren[character] != null)
					return indexedChildren[character];
			}
			else
			{
				if (hashedChildren != null && hashedChildren.TryGetValue(character, out var next))
					return next;
			}

			return Empty;
		}
	}
}