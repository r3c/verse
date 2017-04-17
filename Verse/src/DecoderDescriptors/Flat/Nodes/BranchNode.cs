using System;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Flat.Nodes
{
	internal class BranchNode<TEntity, TContext, TNative> : INode<TEntity, TContext, TNative>
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

		public DecodeAssign<TEntity, TNative> assign = null;

		public BranchNode<TEntity, TContext, TNative>[] branchASCII = null;

		public Dictionary<char, BranchNode<TEntity, TContext, TNative>> branchOther = null;

		public Follow<TEntity, TContext, TNative> enter = null;

		#endregion

		#region Attributes / Static

		private static readonly Container<TEntity, TContext, TNative> blank = new Container<TEntity, TContext, TNative>();

		private static readonly INode<TEntity, TContext, TNative> empty = new EmptyNode<TEntity, TContext, TNative>();

		#endregion

		#region Methods

		public void Assign(ref TEntity target, TNative value)
		{
			if (this.assign != null)
				this.assign(ref target, value);
		}

		public BranchNode<TEntity, TContext, TNative> Connect(char c)
		{
			BranchNode<TEntity, TContext, TNative> next;

			if (c < 128)
			{
				if (this.branchASCII == null)
					this.branchASCII = new BranchNode<TEntity, TContext, TNative>[128];

				if (this.branchASCII[c] != null)
					next = this.branchASCII[c];
				else
				{
					next = new BranchNode<TEntity, TContext, TNative>();

					this.branchASCII[c] = next;
				}
			}
			else
			{
				if (this.branchOther == null)
					this.branchOther = new Dictionary<char, BranchNode<TEntity, TContext, TNative>>();

				if (!this.branchOther.TryGetValue(c, out next))
				{
					next = new BranchNode<TEntity, TContext, TNative>();

					this.branchOther[c] = next;
				}
			}

			return next;
		}

		public bool Enter(ref TEntity target, IReader<TContext, TNative> unknown, TContext context)
		{
			TEntity dummy;

			if (this.enter != null)
				return this.enter(ref target, unknown, context);

			return unknown.ReadValue(() => default(TEntity), BranchNode<TEntity, TContext, TNative>.blank, context, out dummy);
		}

		public INode<TEntity, TContext, TNative> Follow(char c)
		{
			BranchNode<TEntity, TContext, TNative> next;

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

			return BranchNode<TEntity, TContext, TNative>.empty;
		}

		#endregion
	}
}