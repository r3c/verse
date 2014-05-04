
namespace Verse.ParserDescriptors.Recurse.Pointers
{
	class VoidPointer<T, C, V> : IPointer<T, C, V>
	{
		#region Properties

		public bool CanAssign
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Attributes

		public static readonly VoidPointer<T, C, V>  instance = new VoidPointer<T, C, V> ();

		#endregion

		#region Methods

		public void Assign (ref T target, V value)
		{
		}

		public bool Enter (ref T target, IReader<C, V> reader, C context)
		{
			return reader.Read (ref target, this, context);
		}

		public IPointer<T, C, V> Follow (char c)
		{
			return this;
		}

		#endregion
	}
}
