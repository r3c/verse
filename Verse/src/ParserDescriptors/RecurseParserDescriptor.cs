using System;
using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Pointers;

namespace Verse.ParserDescriptors
{
	class RecurseParserDescriptor<T, C, V> : AbstractParserDescriptor<T>
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

		private readonly IDecoder<V>			decoder;

		private readonly NodePointer<T, C, V>	pointer;

		#endregion

		#region Constructors

		public RecurseParserDescriptor (IDecoder<V> decoder)
		{
			this.decoder = decoder;
			this.pointer = new NodePointer<T, C, V> ();
		}

		#endregion

		#region Methods / Public

		public override IParserDescriptor<U> HasField<U> (string name, DescriptorSet<T, U> assign, IParserDescriptor<U> recurse)
		{
			RecurseParserDescriptor<U, C, V>	descriptor;

			descriptor = recurse as RecurseParserDescriptor<U, C, V>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException ("recurse", "invalid target descriptor type");

			this.ConnectField (name, this.EnterInner (descriptor.pointer, assign));

			return recurse;
		}

		public override IParserDescriptor<U> HasField<U> (string name, DescriptorSet<T, U> assign)
		{
			RecurseParserDescriptor<U, C, V>	descriptor;

			descriptor = new RecurseParserDescriptor<U, C, V> (this.decoder);

			this.ConnectField (name, this.EnterInner (descriptor.pointer, assign));

			return descriptor;
		}

		public override IParserDescriptor<T> HasField (string name)
		{
			RecurseParserDescriptor<T, C, V>	descriptor;

			descriptor = new RecurseParserDescriptor<T, C, V> (this.decoder);

			this.ConnectField (name, this.EnterSelf (descriptor.pointer));

			return descriptor;
		}

		public override IParserDescriptor<U> HasItems<U> (DescriptorSet<T, U> append, IParserDescriptor<U> recurse)
		{
			RecurseParserDescriptor<U, C, V>	descriptor;

			descriptor = recurse as RecurseParserDescriptor<U, C, V>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException ("recurse", "invalid target descriptor type");

			this.ConnectChildren (this.EnterInner (descriptor.pointer, append));

			return descriptor;
		}

		public override IParserDescriptor<U> HasItems<U> (DescriptorSet<T, U> append)
		{
			RecurseParserDescriptor<U, C, V>	recurse;

			recurse = new RecurseParserDescriptor<U, C, V> (this.decoder);

			this.ConnectChildren (this.EnterInner (recurse.pointer, append));

			return recurse;
		}

		public override IParserDescriptor<T> HasItems ()
		{
			RecurseParserDescriptor<T, C, V>	descriptor;

			descriptor = new RecurseParserDescriptor<T, C, V> (this.decoder);

			this.ConnectChildren (this.EnterSelf (descriptor.pointer));

			return descriptor;
		}

		public override void IsValue<U> (DescriptorSet<T, U> assign)
		{
			Converter<V, U>	convert;

			if (this.pointer.assign != null)
				throw new InvalidOperationException ("can't declare value assignment twice on same descriptor");

			convert = this.decoder.Get<U> ();

			this.pointer.assign = (ref T target, V value) => assign (ref target, convert (value));
		}

		#endregion

		#region Methods / Private

		private void ConnectChildren (EnterCallback<T, C, V> enter)
		{
			NodePointer<T, C, V>	cycle;

			if (this.pointer.branchDefault != null)
				throw new InvalidOperationException ("can't declare children definition twice on same descriptor");

			cycle = new NodePointer<T, C, V> ();
			cycle.branchDefault = cycle;
			cycle.enter = enter;

			this.pointer.branchDefault = cycle;
		}

		private void ConnectField (string name, EnterCallback<T, C, V> enter)
		{
			NodePointer<T, C, V>	next;

			next = this.pointer;

			foreach (char c in name)
				next = next.Connect (c);

			if (next.enter != null)
				throw new InvalidOperationException ("can't declare same field twice on same descriptor");

			next.enter = enter;
		}

		private EnterCallback<T, C, V> EnterInner<U> (IPointer<U, C, V> child, DescriptorSet<T, U> store)
		{
			DescriptorGet<T, U>	constructor;

			constructor = this.GetConstructor<U> ();

			return (ref T target, IReader<C, V> reader, C context) =>
			{
				U   inner;

				inner = constructor (ref target);

				if (!reader.Read (ref inner, child, context))
					return false;

				store (ref target, inner);

				return true;
			};
		}

		private EnterCallback<T, C, V> EnterSelf (IPointer<T, C, V> child)
		{
			return (ref T target, IReader<C, V> reader, C context) => reader.Read (ref target, child, context);
		}

		#endregion
	}
}
