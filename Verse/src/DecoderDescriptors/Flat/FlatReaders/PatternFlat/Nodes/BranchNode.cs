using System;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Flat.FlatReaders.PatternFlat.Nodes
{
	class BranchNode<TEntity, TState, TValue> : INode<TEntity, TState, TValue>
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

		public DecodeAssign<TEntity, TValue> assign = null;

		public BranchNode<TEntity, TState, TValue>[] branchASCII = null;

		public Dictionary<char, BranchNode<TEntity, TState, TValue>> branchOther = null;

		public ReadEntity<TEntity, TState> enter = null;

		#endregion

		#region Attributes / Static

		private static readonly INode<TEntity, TState, TValue> empty = new EmptyNode<TEntity, TState, TValue>();

		#endregion

		#region Methods

		public void Assign(ref TEntity target, TValue value)
		{
			if (this.assign != null)
				this.assign(ref target, value);
		}

		public BranchNode<TEntity, TState, TValue> Connect(char c)
		{
			BranchNode<TEntity, TState, TValue> next;

			if (c < 128)
			{
				if (this.branchASCII == null)
					this.branchASCII = new BranchNode<TEntity, TState, TValue>[128];

				if (this.branchASCII[c] != null)
					next = this.branchASCII[c];
				else
				{
					next = new BranchNode<TEntity, TState, TValue>();

					this.branchASCII[c] = next;
				}
			}
			else
			{
				if (this.branchOther == null)
					this.branchOther = new Dictionary<char, BranchNode<TEntity, TState, TValue>>();

				if (!this.branchOther.TryGetValue(c, out next))
				{
					next = new BranchNode<TEntity, TState, TValue>();

					this.branchOther[c] = next;
				}
			}

			return next;
		}

		public bool Enter(ref TEntity target, IFlatReader<TEntity, TState, TValue> unknown, TState context)
		{
			TEntity dummy;

			if (this.enter != null)
				return this.enter(ref target, context);

			return unknown.ReadValue(() => default(TEntity), context, out dummy);
		}

		public INode<TEntity, TState, TValue> Follow(char c)
		{
			BranchNode<TEntity, TState, TValue> next;

			if (c < 128)
			{
				if (this.branchASCII != null && this.branchASCII[c] != null)
					return this.branchASCII[c];
			}
			else
			{
				if (this.branchOther != null && this.branchOther.TryGetValue(c, out next))
					return next;
			}

			return BranchNode<TEntity, TState, TValue>.empty;
		}

		#endregion
	}
}