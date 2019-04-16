namespace Verse
{
	internal interface ILookup<in TKey, TValue>
	{
		bool HasValue { get; }

		TValue Value { get; }

		ILookup<TKey, TValue> Follow(TKey key);
	}
}
