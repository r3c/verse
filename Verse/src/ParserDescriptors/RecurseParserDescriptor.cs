using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Recurse;

namespace Verse.ParserDescriptors
{
    sealed class RecurseParserDescriptor<T, C, V> : AbstractParserDescriptor<T>
    {
    	#region Properties

        public IPointer<T, C, V> Pointer
        {
            get
            {
                return this.pointer;
            }
        }

        #endregion

        #region Attributes

        private readonly IAdapter<V>	adapter;

        private readonly NodePointer	pointer;

        #endregion

        #region Constructors

        public RecurseParserDescriptor (IAdapter<V> adapter)
        {
        	this.adapter = adapter;
            this.pointer = new NodePointer ();
        }

        #endregion

        #region Methods / Public

        public override IParserDescriptor<U> HasChildren<U> (DescriptorSet<T, U> store, DescriptorGet<T, U> create, IParserDescriptor<U> recurse)
        {
        	RecurseParserDescriptor<U, C, V>	descriptor;

        	descriptor = recurse as RecurseParserDescriptor<U, C, V>;

        	if (descriptor == null)
        		throw new ArgumentOutOfRangeException ("recurse", "invalid target descriptor type");

        	this.ConnectChildren<T> (this.EnterInner (descriptor.pointer, store, create));

        	return descriptor;
        }

        public override IParserDescriptor<U> HasChildren<U> (DescriptorSet<T, U> store, DescriptorGet<T, U> create)
        {
        	RecurseParserDescriptor<U, C, V>	recurse;

        	recurse = new RecurseParserDescriptor<U, C, V> (this.adapter);

        	this.ConnectChildren<U> (this.EnterInner (recurse.pointer, store, create));

        	return recurse;
        }

        public override IParserDescriptor<T> HasChildren ()
        {
        	RecurseParserDescriptor<T, C, V>	descriptor;

        	descriptor = new RecurseParserDescriptor<T, C, V> (this.adapter);

        	this.ConnectChildren<T> (this.EnterSelf (descriptor.pointer));

        	return descriptor;
        }

        public override IParserDescriptor<U> HasField<U> (string name, DescriptorSet<T, U> store, DescriptorGet<T, U> create, IParserDescriptor<U> recurse)
        {
        	RecurseParserDescriptor<U, C, V>	descriptor;

        	descriptor = recurse as RecurseParserDescriptor<U, C, V>;

        	if (descriptor == null)
        		throw new ArgumentOutOfRangeException ("recurse", "invalid target descriptor type");

        	this.ConnectField<T> (name, this.EnterInner (descriptor.pointer, store, create));

        	return recurse;
        }

        public override IParserDescriptor<U> HasField<U> (string name, DescriptorSet<T, U> store, DescriptorGet<T, U> create)
        {
        	RecurseParserDescriptor<U, C, V>	descriptor;

        	descriptor = new RecurseParserDescriptor<U, C, V> (this.adapter);

        	this.ConnectField<U> (name, this.EnterInner (descriptor.pointer, store, create));

        	return descriptor;
        }

        public override IParserDescriptor<T> HasField (string name)
        {
        	RecurseParserDescriptor<T, C, V>	descriptor;

        	descriptor = new RecurseParserDescriptor<T, C, V> (this.adapter);

        	this.ConnectField<T> (name, this.EnterSelf (descriptor.pointer));

        	return descriptor;
        }

		public override void IsValue<U> (DescriptorSet<T, U> store)
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

        private NodeCallback EnterInner<U> (IPointer<U, C, V> child, DescriptorSet<T, U> store, DescriptorGet<T, U> create)
        {
            return (ref T target, IReader<C, V> reader, C context) =>
            {
                U   inner;

                inner = create (ref target);

                if (!reader.Read (ref inner, child, context))
					return false;

                store (ref target, inner);

                return true;
            };
        }

        private NodeCallback EnterSelf (IPointer<T, C, V> child)
        {
        	return (ref T target, IReader<C, V> reader, C context) => reader.Read (ref target, child, context);
        }

        #endregion

        #region Types

        private delegate bool NodeCallback (ref T target, IReader<C, V> reader, C context);

        private class NodePointer : IPointer<T, C, V>
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

            public bool Enter (ref T target, IReader<C, V> reader, C context)
            {
                if (this.enter != null)
                    return this.enter (ref target, reader, context);

                return reader.Read (ref target, VoidPointer.instance, context);
            }
            
            public IPointer<T, C, V> Next (char c)
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

        private class VoidPointer : IPointer<T, C, V>
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

            public bool Enter (ref T target, IReader<C, V> reader, C context)
            {
                return reader.Read (ref target, this, context);
            }

            public IPointer<T, C, V> Next (char c)
            {
                return this;
            }
        }

        #endregion
    }
}
