using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Recurse;

namespace Verse.ParserDescriptors.Recurse.Readers.String.Nodes
{
    class BranchNode<TEntity, TValue, TState> : INode<TEntity, TValue, TState>
    {
        public bool HasSubNode
        {
            get
            {
                return this.hasSubNode;
            }
        }

        public bool IsConnected
        {
            get
            {
                return true;
            }
        }

        #region Attributes

        public ParserAssign<TEntity, TValue> assign = null;

        public BranchNode<TEntity, TValue, TState>[] branchASCII = null;

        public Dictionary<char, BranchNode<TEntity, TValue, TState>> branchOther = null;

        public Enter<TEntity, TState> enter = null;

        private bool hasSubNode = false;

        #endregion

        #region Methods

        public void Assign(ref TEntity target, TValue value)
        {
            if (this.assign != null)
                this.assign(ref target, value);
        }

        public BranchNode<TEntity, TValue, TState> Connect(char c)
        {
            BranchNode<TEntity, TValue, TState> next;

            this.hasSubNode = true;

            if (c < 128)
            {
                if (this.branchASCII == null)
                    this.branchASCII = new BranchNode<TEntity, TValue, TState>[128];

                if (this.branchASCII[c] != null)
                    next = this.branchASCII[c];
                else
                {
                    next = new BranchNode<TEntity, TValue, TState>();

                    this.branchASCII[c] = next;
                }
            }
            else
            {
                if (this.branchOther == null)
                    this.branchOther = new Dictionary<char, BranchNode<TEntity, TValue, TState>>();

                if (!this.branchOther.TryGetValue(c, out next))
                {
                    next = new BranchNode<TEntity, TValue, TState>();

                    this.branchOther[c] = next;
                }
            }

            return next;
        }

        public bool Enter(ref TEntity target, IReader<TEntity, TValue, TState> unknown, TState state)
        {
            if (this.enter != null)
                return this.enter(ref target, state);

            return unknown.ReadValue(ref target, state);
        }

        public INode<TEntity, TValue, TState> Follow(char c)
        {
            BranchNode<TEntity, TValue, TState> next;

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

            return EmptyNode<TEntity, TValue, TState>.Instance;
        }

        #endregion
    }
}