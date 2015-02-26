using System;
using System.Collections.Generic;

namespace Verse.ParserDescriptors.Recurse.Nodes
{
	class BranchNode<T, C, V> : INode<T, C, V>
	{
		#region Properties

		public bool CanAssign
		{
			get
			{
				return this.assign != null;
			}
		}

		#endregion

		#region Attributes / Instance

		public ParserAssign<T, V> assign = null;

		public BranchNode<T, C, V>[] branchASCII = null;

		public Dictionary<char, BranchNode<T, C, V>> branchOther = null;

		public Follow<T, C, V> enter = null;

		#endregion

		#region Attributes / Static

		private static readonly Container<T, C, V> blank = new Container<T, C, V> ();

		private static readonly INode<T, C, V> empty = new EmptyNode<T, C, V> ();

		#endregion

		#region Methods

		public void Assign (ref T target, V value)
		{
			if (this.assign != null)
				this.assign (ref target, value);
		}

		public BranchNode<T, C, V> Connect (char c)
		{
			BranchNode<T, C, V> next;

			if (c < 128)
			{
				if (this.branchASCII == null)
					this.branchASCII = new BranchNode<T, C, V>[128];

				if (this.branchASCII[c] != null)
					next = this.branchASCII[c];
				else
				{
					next = new BranchNode<T, C, V> ();

					this.branchASCII[c] = next;
				}
			}
			else
			{
				if (this.branchOther == null)
					this.branchOther = new Dictionary<char, BranchNode<T, C, V>> ();

				if (!this.branchOther.TryGetValue (c, out next))
				{
					next = new BranchNode<T, C, V> ();

					this.branchOther[c] = next;
				}
			}

			return next;
		}

		public bool Enter (ref T target, IReader<C, V> reader, C context)
		{
			if (this.enter != null)
				return this.enter (ref target, reader, context);

			return reader.ReadValue (ref target, BranchNode<T, C, V>.blank, context);
		}

		public INode<T, C, V> Follow (char c)
		{
			BranchNode<T, C, V> next;

			if (c < 128)
			{
				if (this.branchASCII != null && this.branchASCII[c] != null)
					return this.branchASCII[c];
			}
			else
			{
				if (this.branchOther != null && this.branchOther.TryGetValue (c, out next))
					return next;
			}

			return BranchNode<T, C, V>.empty;
		}

		#endregion
	}
}
