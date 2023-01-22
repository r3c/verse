namespace Verse
{
	/// <summary>
	/// Delegate used as field setter on a given entity.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	/// <typeparam name="TValue">Value type</typeparam>
	/// <param name="target">Target entity</param>
	/// <param name="value">Source value</param>
	public delegate void Setter<TEntity, in TValue>(ref TEntity target, TValue value);
}