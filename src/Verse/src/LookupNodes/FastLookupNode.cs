namespace Verse.LookupNodes
{
	internal class FastLookupNode<TKey, TValue> : ILookupNode<TKey, TValue>
	{
		public bool HasValue => this.shortcut.HasValue || this.fallback.HasValue;
		public TValue Value => this.shortcut.HasValue ? this.shortcut.Value : this.fallback.Value;

		private readonly ILookupNode<TKey, TValue> fallback;
		private readonly ILookupNode<TKey, TValue> shortcut;

		public FastLookupNode(ILookupNode<TKey, TValue> shortcut, ILookupNode<TKey, TValue> fallback)
		{
			this.fallback = fallback;
			this.shortcut = shortcut;
		}

		public ILookupNode<TKey, TValue> Follow(TKey key)
		{
			var direct = this.shortcut.Follow(key);

			return direct.HasValue ? direct : this.fallback.Follow(key);
		}
	}
}