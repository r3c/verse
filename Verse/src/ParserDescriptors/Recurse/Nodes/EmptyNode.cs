
namespace Verse.ParserDescriptors.Recurse.Nodes
{
	class EmptyNode<T, C, V> : INode<T, C, V>
	{
		#region Properties

		public bool	CanAssign
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Attributes

		private static readonly Container<T, C, V>	blank = new Container<T, C, V> ();

		#endregion

		#region Methods

		public void Assign (ref T target, V value)
		{
		}

		public bool Enter (ref T target, IReader<C, V> reader, C context)
		{
			return reader.Read (ref target, EmptyNode<T, C, V>.blank, context);
		}

		public INode<T, C, V> Follow (char c)
		{
			return this;
		}

		#endregion
	}
}
