using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.ParserDescriptors
{
	// FIXME: may be moved to namespace Verse.ParserDescriptors.Recurse
    public static class RecurseParserDescriptor
    {
        #region Types

        public interface IAdapter<V>
        {
        	Converter<V, T>	Get<T> ();
        }

        public interface IPointer<T, C, V>
        {
            bool	CanAssign
            {
                get;
            }

            void				Assign (ref T target, V value);

            bool				Enter (ref T target, IReader<C, V> reader, C context);

            IPointer<T, C, V>	Next (char c);
        }

        public interface IReader<C, V>
        {
            bool    Begin (Stream stream, out C context);

            void	End (C context);

            bool    Read<T> (ref T target, IPointer<T, C, V> pointer, C context);
        }

        #endregion
    }

    sealed class RecurseParserDescriptor<T, C, V> : AbstractParserDescriptor<T>
    {
    	#region Properties

        public RecurseParserDescriptor.IPointer<T, C, V> Pointer
        {
            get
            {
                return this.pointer;
            }
        }

        #endregion

        #region Attributes

        private readonly RecurseParserDescriptor.IAdapter<V>	adapter;

        private readonly NodePointer							pointer;

        #endregion

        #region Constructors

        public RecurseParserDescriptor (RecurseParserDescriptor.IAdapter<V> adapter)
        {
        	this.adapter = adapter;
            this.pointer = new NodePointer ();
        }

        #endregion

        #region Methods / Public

        public override IParserDescriptor<U> ForChildren<U> (DescriptorSet<T, U> assign, DescriptorGet<T, U> create)
        {
        	RecurseParserDescriptor<U, C, V>	recurse;

        	recurse = new RecurseParserDescriptor<U, C, V> (this.adapter);

        	this.ConnectChildren<U> (this.EnterInner (recurse.pointer, assign, create));

        	return recurse;
        }

        public override IParserDescriptor<T> ForChildren (IParserDescriptor<T> descriptor)
        {
        	RecurseParserDescriptor<T, C, V>	recurse;

        	recurse = descriptor as RecurseParserDescriptor<T, C, V>;

        	if (recurse == null)
        		throw new ArgumentOutOfRangeException ("descriptor", "invalid target descriptor type");

        	this.ConnectChildren<T> (this.EnterSelf (recurse.pointer));

        	return descriptor;
        }

        public override IParserDescriptor<U> ForField<U> (string name, DescriptorSet<T, U> assign, DescriptorGet<T, U> create)
        {
        	RecurseParserDescriptor<U, C, V>	recurse;

        	recurse = new RecurseParserDescriptor<U, C, V> (this.adapter);

        	this.ConnectField<U> (name, this.EnterInner (recurse.pointer, assign, create));

        	return recurse;
        }

        public override IParserDescriptor<T> ForField (string name, IParserDescriptor<T> descriptor)
        {
        	RecurseParserDescriptor<T, C, V>	recurse;

        	recurse = descriptor as RecurseParserDescriptor<T, C, V>;

        	if (recurse == null)
        		throw new ArgumentOutOfRangeException ("descriptor", "invalid target descriptor type");

        	this.ConnectField<T> (name, this.EnterSelf (recurse.pointer));

        	return descriptor;
        }

        public override void ForValue<U> (DescriptorSet<T, U> store)
        {
        	Converter<V, U>	convert;

            if (this.pointer.assign != null)
                throw new InvalidOperationException ("cannot create value assignment twice on same descriptor");

            convert = this.adapter.Get<U> ();

            this.pointer.assign = (ref T target, V value) => store (ref target, convert (value));
        }

        #endregion

        #region Methods / Private

        private void ConnectChildren<U> (NodeCallback enter)
        {
            NodePointer	cycle;

            if (this.pointer.branchDefault != null)
                throw new InvalidOperationException ("can't declare children definition twice on same descriptor");

            cycle = new NodePointer ();
            cycle.branchDefault = cycle;
            cycle.enter = enter;

            this.pointer.branchDefault = cycle;
        }

        private void ConnectField<U> (string name, NodeCallback enter)
        {
            NodePointer	current;
            NodePointer	next;

            current = this.pointer;

            foreach (char c in name)
            {
                if (c >= 128)
                {
                	if (current.branchMap == null)
                		current.branchMap = new Dictionary<char, NodePointer> ();

                    if (!current.branchMap.TryGetValue (c, out next))
                    {
                        next = new NodePointer ();

                        current.branchMap[c] = next;
                    }
                }
                else
                {
                	if (current.branchTable == null)
                		current.branchTable = new NodePointer[128];

                    if (current.branchTable[c] != null)
                        next = current.branchTable[c];
                    else
                    {
                        next = new NodePointer ();

                        current.branchTable[c] = next;
                    }
                }

                current = next;
            }

            if (current.enter != null)
                throw new InvalidOperationException ("can't declare field twice on same descriptor");

            current.enter = enter;
        }

        private NodeCallback EnterInner<U> (RecurseParserDescriptor.IPointer<U, C, V> child, DescriptorSet<T, U> assign, DescriptorGet<T, U> create)
        {
            return (ref T target, RecurseParserDescriptor.IReader<C, V> reader, C context) =>
            {
                U   inner;

                inner = create (ref target);

                if (!reader.Read (ref inner, child, context))
					return false;

                assign (ref target, inner);

                return true;
            };
        }

        private NodeCallback EnterSelf (RecurseParserDescriptor.IPointer<T, C, V> child)
        {
        	return (ref T target, RecurseParserDescriptor.IReader<C, V> reader, C context) => reader.Read (ref target, child, context);
        }

        #endregion

        #region Types

        private delegate bool NodeCallback (ref T target, RecurseParserDescriptor.IReader<C, V> reader, C context);

        private class NodePointer : RecurseParserDescriptor.IPointer<T, C, V>
        {
            public bool CanAssign
            {
                get
                {
                    return this.assign != null;
                }
            }

            public DescriptorSet<T, V>				assign = null;

            public NodePointer						branchDefault = null;

            public Dictionary<char, NodePointer>	branchMap = null;

            public NodePointer[]					branchTable = null;

            public NodeCallback						enter = null;

            public void Assign (ref T target, V value)
            {
            	if (this.assign != null)
            		this.assign (ref target, value);
            }

            public bool Enter (ref T target, RecurseParserDescriptor.IReader<C, V> reader, C context)
            {
                if (this.enter != null)
                    return this.enter (ref target, reader, context);

                return reader.Read (ref target, VoidPointer.instance, context);
            }
            
            public RecurseParserDescriptor.IPointer<T, C, V> Next (char c)
            {
                NodePointer next;

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

                return VoidPointer.instance;
            }
        }

        private class VoidPointer : RecurseParserDescriptor.IPointer<T, C, V>
        {
            public bool CanAssign
            {
                get
                {
                    return false;
                }
            }

            public static readonly VoidPointer  instance = new VoidPointer ();

            public void Assign (ref T target, V value)
            {
            }

            public bool Enter (ref T target, RecurseParserDescriptor.IReader<C, V> reader, C context)
            {
                return reader.Read (ref target, this, context);
            }

            public RecurseParserDescriptor.IPointer<T, C, V> Next (char c)
            {
                return this;
            }
        }

        #endregion
    }
}
