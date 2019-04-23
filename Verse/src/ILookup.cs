namespace Verse
{
	internal interface ILookup<in TKey, out TValue>
	{
		bool HasValue { get; }

		TValue Value { get; }

		ILookup<TKey, TValue> Follow(TKey key);
	}
}
