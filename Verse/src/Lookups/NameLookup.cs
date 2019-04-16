using System.Collections.Generic;

namespace Verse.Lookups
{
	internal static class NameLookup
	{
		public const int HashThreshold = 128;
	}

	internal class NameLookup<TValue> : ILookup<int, TValue>
	{
		public static readonly ILookup<int, TValue> Empty = new NameLookup<TValue>();

		public bool HasValue { get; private set; }

		public TValue Value { get; private set; }

		private Dictionary<int, NameLookup<TValue>> hashedChildren;

		private NameLookup<TValue>[] indexedChildren;

		public NameLookup()
		{
			this.HasValue = false;
			this.Value = default;
		}

		public bool ConnectTo(string sequence, TValue value)
		{
			var current = this;

			foreach (var key in sequence)
			{
				NameLookup<TValue> next;

				if (key < NameLookup.HashThreshold)
				{
					if (current.indexedChildren == null)
						current.indexedChildren = new NameLookup<TValue>[NameLookup.HashThreshold];

					if (current.indexedChildren[key] != null)
						next = current.indexedChildren[key];
					else
					{
						next = new NameLookup<TValue>();

						current.indexedChildren[key] = next;
					}
				}
				else
				{
					if (current.hashedChildren == null)
						current.hashedChildren = new Dictionary<int, NameLookup<TValue>>();

					if (!current.hashedChildren.TryGetValue(key, out next))
					{
						next = new NameLookup<TValue>();

						current.hashedChildren[key] = next;
					}
				}

				current = next;
			}

			if (current.HasValue)
				return false;

			current.HasValue = true;
			current.Value = value;

			return true;
		}

		public ILookup<int, TValue> Follow(int key)
		{
			if (key < NameLookup.HashThreshold)
			{
				if (this.indexedChildren != null && this.indexedChildren[key] != null)
					return this.indexedChildren[key];
			}
			else
			{
				if (this.hashedChildren != null && this.hashedChildren.TryGetValue(key, out var next))
					return next;
			}

			return NameLookup<TValue>.Empty;
		}
	}
}
