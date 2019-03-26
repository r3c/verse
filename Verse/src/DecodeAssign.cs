namespace Verse
{
	/// <summary>
	/// Value to entity (passed by reference) assignment delegate.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	/// <typeparam name="TValue">Value type</typeparam>
	/// <param name="target">Target entity</param>
	/// <param name="value">Source value</param>
	public delegate void DecodeAssign<TEntity, in TValue>(ref TEntity target, TValue value);
}