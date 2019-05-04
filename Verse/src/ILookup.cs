namespace Verse
{
	internal interface ILookup<in TKey, TValue>
	{
		ILookupNode<TKey, TValue> Root { get; }

		bool ConnectTo(string sequence, TValue value);
	}
}
