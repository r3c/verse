using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Recurse;

namespace Verse.ParserDescriptors.Recurse.Readers.Pattern.Nodes
{
    class BranchNode<TEntity, TValue, TState> : INode<TEntity, TValue, TState>
    {
        #region Constants

        private const int INDEX_SIZE = 128;

        #endregion

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

        public BranchNode<TEntity, TValue, TState>[] branchIndex = null;

        public Dictionary<char, BranchNode<TEntity, TValue, TState>> branchHash = null;

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

            if (c < INDEX_SIZE)
            {
                if (this.branchIndex == null)
                    this.branchIndex = new BranchNode<TEntity, TValue, TState>[INDEX_SIZE];

                if (this.branchIndex[c] != null)
                    next = this.branchIndex[c];
                else
                {
                    next = new BranchNode<TEntity, TValue, TState>();

                    this.branchIndex[c] = next;
                }
            }
            else
            {
                if (this.branchHash == null)
                    this.branchHash = new Dictionary<char, BranchNode<TEntity, TValue, TState>>();

                if (!this.branchHash.TryGetValue(c, out next))
                {
                    next = new BranchNode<TEntity, TValue, TState>();

                    this.branchHash[c] = next;
                }
            }

            return next;
        }

        public bool Enter(ref TEntity target, IReader<TEntity, TValue, TState> unknown, TState state)
        {
            if (this.enter != null)
                return this.enter(ref target, state);

            return unknown.ReadEntity(ref target, state);
        }

        public INode<TEntity, TValue, TState> Follow(char c)
        {
            BranchNode<TEntity, TValue, TState> next;

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

            return EmptyNode<TEntity, TValue, TState>.Instance;
        }

        #endregion
    }
}