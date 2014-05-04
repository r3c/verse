using System;
using System.Collections.Generic;

namespace Verse.ParserDescriptors.Recurse.Pointers
{
	class NodePointer<T, C, V> : IPointer<T, C, V>
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

		#region Attributes

		public DescriptorSet<T, V>						assign = null;

		public NodePointer<T, C, V>						branchDefault = null;

		public Dictionary<char, NodePointer<T, C, V>>	branchMap = null;

		public NodePointer<T, C, V>[]					branchTable = null;

		public EnterCallback<T, C, V>					enter = null;

		#endregion

		#region Methods

		public void Assign (ref T target, V value)
		{
			if (this.assign != null)
				this.assign (ref target, value);
		}

		public NodePointer<T, C, V> Connect (char c)
		{
			NodePointer<T, C, V>	next;

			if (c >= 128)
			{
				if (this.branchMap == null)
					this.branchMap = new Dictionary<char, NodePointer<T, C, V>> ();

				if (!this.branchMap.TryGetValue (c, out next))
				{
					next = new NodePointer<T, C, V> ();

					this.branchMap[c] = next;
				}
			}
			else
			{
				if (this.branchTable == null)
					this.branchTable = new NodePointer<T, C, V>[128];

				if (this.branchTable[c] != null)
					next = this.branchTable[c];
				else
				{
					next = new NodePointer<T, C, V> ();

					this.branchTable[c] = next;
				}
			}

			return next;
		}

		public bool Enter (ref T target, IReader<C, V> reader, C context)
		{
			if (this.enter != null)
				return this.enter (ref target, reader, context);

			return reader.Read (ref target, VoidPointer<T, C, V>.instance, context);
		}
		
		public IPointer<T, C, V> Follow (char c)
		{
			NodePointer<T, C, V>	next;

			if (c >= 128)
			{
				if (this.branchMap != null && this.branchMap.TryGetValue (c, out next))
					return next;
			}
			else if (this.branchTable != null)
			{
				if (this.branchTable[c] != null)
					return this.branchTable[c];
			}

			if (this.branchDefault != null)
				return this.branchDefault;

			return VoidPointer<T, C, V>.instance;
		}

		#endregion
	}
}
