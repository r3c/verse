using System;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Abstract
{
	class EntityTree<TEntity, TState>
	{
		#region Constants

		private const int INDEX_SIZE = 128;

		#endregion

		public EntityReader<TEntity, TState> Read
		{
			get
			{
				return this.read;
			}
		}

		public static readonly EntityTree<TEntity, TState> Empty = new EntityTree<TEntity, TState>();

		#region Attributes

		private EntityTree<TEntity, TState>[] branchIndex = null;

		private Dictionary<char, EntityTree<TEntity, TState>> branchHash = null;

		private EntityReader<TEntity, TState> read;

		#endregion

		#region Methods

		public bool Connect(string name, EntityReader<TEntity, TState> read)
		{
			EntityTree<TEntity, TState> current = this;
			EntityTree<TEntity, TState> next = this;

			foreach (char c in name)
			{
				if (c < INDEX_SIZE)
				{
					if (current.branchIndex == null)
						current.branchIndex = new EntityTree<TEntity, TState>[INDEX_SIZE];
	
					if (current.branchIndex[c] != null)
						next = current.branchIndex[c];
					else
					{
						next = new EntityTree<TEntity, TState>();
	
						current.branchIndex[c] = next;
					}
				}
				else
				{
					if (current.branchHash == null)
						current.branchHash = new Dictionary<char, EntityTree<TEntity, TState>>();
	
					if (!current.branchHash.TryGetValue(c, out next))
					{
						next = new EntityTree<TEntity, TState>();
	
						current.branchHash[c] = next;
					}
				}

				current = next;
			}

			if (next.read != null)
				return false;

			next.read = read;

			return true;
		}

		public EntityTree<TEntity, TState> Follow(char c)
		{
			EntityTree<TEntity, TState> next;

			if (c < INDEX_SIZE)
			{
				if (this.branchIndex != null && this.branchIndex[c] != null)
					return this.branchIndex[c];
			}
			else
			{
				if (this.branchHash != null && this.branchHash.TryGetValue(c, out next))
					return next;
			}

			return EntityTree<TEntity, TState>.Empty;
		}

		#endregion
	}
}
