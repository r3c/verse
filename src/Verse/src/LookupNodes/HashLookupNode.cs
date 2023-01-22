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
			var character = this.extractor(key);

			if (character < HashLookupNode.HashThreshold)
			{
				if (this.indexedChildren == null)
					this.indexedChildren = new HashLookupNode<TKey, TValue>[HashLookupNode.HashThreshold];

				if (this.indexedChildren[character] != null)
					return this.indexedChildren[character];

				var next = new HashLookupNode<TKey, TValue>(this.extractor);

				this.indexedChildren[character] = next;

				return next;
			}
			else
			{
				if (this.hashedChildren == null)
					this.hashedChildren = new Dictionary<int, HashLookupNode<TKey, TValue>>();

				if (this.hashedChildren.TryGetValue(character, out var next))
					return next;

				next = new HashLookupNode<TKey, TValue>(this.extractor);

				this.hashedChildren[character] = next;

				return next;
			}
		}

		public ILookupNode<TKey, TValue> Follow(TKey key)
		{
			var character = this.extractor(key);

			if (character < HashLookupNode.HashThreshold)
			{
				if (this.indexedChildren != null && this.indexedChildren[character] != null)
					return this.indexedChildren[character];
			}
			else
			{
				if (this.hashedChildren != null && this.hashedChildren.TryGetValue(character, out var next))
					return next;
			}

			return HashLookupNode<TKey, TValue>.Empty;
		}
	}
}