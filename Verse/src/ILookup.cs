namespace Verse
{
	internal interface ILookup<in TKey, TValue>
	{
		bool HasValue { get; }

		TValue Value { get; }

		bool ConnectTo(string sequence, TValue value);

		ILookup<TKey, TValue> Follow(TKey key);
	}
}
