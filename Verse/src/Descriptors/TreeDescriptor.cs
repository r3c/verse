using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.Descriptors
{
    public sealed class TreeDescriptor<C, V>
    {
        #region Types

        public interface IBuilder<T>
        {
            bool        CanAssign
            {
                get;
            }

            bool        Assign(ref T target, V value);

            bool        Enter(ref T target, IReader reader, C context);

            IBuilder<T> Next(char c);
        }

        public interface IReader
        {
            bool    Initialize(Stream stream, out C context);

            bool    Read<T>(ref T target, IBuilder<T> builder, C context);
        }

        #endregion
    }

    sealed class TreeDescriptor<T, C, V> : AbstractDescriptor<T>
    {
        public TreeDescriptor<C, V>.IBuilder<T> Builder
        {
            get
            {
                return this.builder;
            }
        }

        private NodeBuilder builder;

        public TreeDescriptor()
        {
            this.builder = new NodeBuilder();
        }

        private TreeDescriptor(NodeBuilder builder)
        {
            this.builder = builder;
        }

        #region Methods / Public

        public override IDescriptor<U> ForChildren<U>(DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create)
        {
            return this.ConnectChildren<U>((child) => this.EnterInner(child, assign, create));
        }

        public override IDescriptor<T> ForChildren()
        {
            return this.ConnectChildren<T>((child) => this.EnterSelf(child));
        }

        public override IDescriptor<U> ForField<U>(string name, DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create)
        {
            return this.ConnectField<U>(name, (child) => this.EnterInner(child, assign, create));
        }

        public override IDescriptor<T> ForField(string name)
        {
            return this.ConnectField<T>(name, (child) => this.EnterSelf(child));
        }

        public override void LetValue<U>(DescriptorAssign<T, U> assign)
        {
            if (this.builder.assign != null)
                throw new InvalidOperationException("cannot create value assignment twice on same descriptor");

            // FIXME: find converter, generate DescriptorAssign<T, V>
            Converter<V, U> convert = typeof(U) == typeof(V) ? (Converter<V, U>)(object)(new Converter<V, V>((i) => i)) : null;

            this.builder.assign = (ref T target, V value) => assign(ref target, convert(value));
        }

        #endregion

        #region Methods / Private

        private IDescriptor<U> ConnectChildren<U>(Func<TreeDescriptor<U, C, V>.NodeBuilder, NodeBuilderEnter> enter)
        {
            TreeDescriptor<U, C, V>.NodeBuilder child;
            NodeBuilder                         cycle;

            if (this.builder.branchDefault != null)
                throw new InvalidOperationException("can't declare children definition twice on same descriptor");

            child = new TreeDescriptor<U, C, V>.NodeBuilder();

            cycle = new NodeBuilder();
            cycle.branchDefault = cycle;
            cycle.enter = enter(child);

            this.builder.branchDefault = cycle;

            return new TreeDescriptor<U, C, V>(child);
        }

        private IDescriptor<U> ConnectField<U>(string name, Func<TreeDescriptor<U, C, V>.NodeBuilder, NodeBuilderEnter> enter)
        {
            TreeDescriptor<U, C, V>.NodeBuilder child;
            NodeBuilder                         current;
            NodeBuilder                         next;

            current = this.builder;

            foreach (char c in name)
            {
                if (c >= 128)
                {
                    if (!current.branchMap.TryGetValue(c, out next))
                    {
                        next = new NodeBuilder();

                        current.branchMap[c] = next;
                    }
                }
                else
                {
                    if (current.branchTable[c] != null)
                        next = current.branchTable[c];
                    else
                    {
                        next = new NodeBuilder();

                        current.branchTable[c] = next;
                    }
                }

                current = next;
            }

            if (current.enter != null)
                throw new InvalidOperationException("can't declare field twice on same descriptor");

            child = new TreeDescriptor<U, C, V>.NodeBuilder();

            current.enter = enter(child);

            return new TreeDescriptor<U, C, V>(child);
        }

        private NodeBuilderEnter EnterInner<U>(TreeDescriptor<C, V>.IBuilder<U> child, DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create)
        {
            return (ref T target, TreeDescriptor<C, V>.IReader reader, C context) =>
            {
                U   value;

                value = create(ref target);

                if (!reader.Read(ref value, child, context))
                    return false;

                assign(ref target, value);

                return true;
            };
        }

        private NodeBuilderEnter EnterSelf(TreeDescriptor<C, V>.IBuilder<T> child)
        {
            return (ref T target, TreeDescriptor<C, V>.IReader reader, C context) =>
            {
                return reader.Read(ref target, child, context);
            };
        }

        #endregion

        #region Types

        private delegate bool NodeBuilderEnter(ref T target, TreeDescriptor<C, V>.IReader reader, C context);

        private class NodeBuilder : TreeDescriptor<C, V>.IBuilder<T>
        {
            public bool CanAssign
            {
                get
                {
                    return this.assign != null;
                }
            }

            public DescriptorAssign<T, V>           assign = null;

            public NodeBuilder                      branchDefault = null;

            public Dictionary<char, NodeBuilder>    branchMap = new Dictionary<char, NodeBuilder>();

            public NodeBuilder[]                    branchTable = new NodeBuilder[128];

            public NodeBuilderEnter                 enter = null;

            public bool Assign(ref T target, V value)
            {
                if (this.assign != null)
                    return this.assign(ref target, value);

                return true;
            }

            public bool Enter(ref T target, TreeDescriptor<C, V>.IReader reader, C context)
            {
                if (this.enter != null)
                    return this.enter(ref target, reader, context);

                return reader.Read(ref target, VoidBuilder.instance, context);
            }
            
            public TreeDescriptor<C, V>.IBuilder<T> Next(char c)
            {
                NodeBuilder next;

                if (c >= 128)
                {
                    if (this.branchMap.TryGetValue(c, out next))
                        return next;
                }
                else
                {
                    if (this.branchTable[c] != null)
                        return this.branchTable[c];
                }

                if (this.branchDefault != null)
                    return this.branchDefault;

                return VoidBuilder.instance;
            }
        }

        private class VoidBuilder : TreeDescriptor<C, V>.IBuilder<T>
        {
            public bool CanAssign
            {
                get
                {
                    return false;
                }
            }

            public static readonly VoidBuilder  instance = new VoidBuilder();

            public bool Assign(ref T target, V value)
            {
                return true;
            }

            public bool Enter(ref T target, TreeDescriptor<C, V>.IReader reader, C context)
            {
                return reader.Read(ref target, this, context);
            }

            public TreeDescriptor<C, V>.IBuilder<T> Next(char c)
            {
                return this;
            }
        }

        #endregion
    }
}
