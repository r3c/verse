using System;
using System.Collections.Generic;
using Verse.BuilderDescriptors.Recurse;
using Verse.BuilderDescriptors.Recurse.Pointers;

namespace Verse.BuilderDescriptors
{
	class RecurseBuilderDescriptor<T, C, V> : AbstractBuilderDescriptor<T>
	{
		public IPointer<T, C, V>	Pointer
		{
			get
			{
				return this.pointer;
			}
		}

		private readonly IEncoder<V>			encoder;

		private readonly NodePointer<T, C, V>	pointer;

		public RecurseBuilderDescriptor (IEncoder<V> encoder)
		{
			this.encoder = encoder;
			this.pointer = new NodePointer<T, C, V> ();
		}

		public override IBuilderDescriptor<U> HasField<U> (string name, Func<T, U> access, IBuilderDescriptor<U> recurse)
		{
			throw new System.NotImplementedException ();
		}

		public override IBuilderDescriptor<U> HasField<U> (string name, Func<T, U> access)
		{
			RecurseBuilderDescriptor<U, C, V>	descriptor;
			NodePointer<U, C, V>				inner;

			descriptor = new RecurseBuilderDescriptor<U, C, V> (this.encoder);
			inner = descriptor.pointer;

			this.pointer.fields[name] = (source, writer, context) => writer.WriteKey (access (source), name, inner, context);

			return descriptor;
		}

		public override IBuilderDescriptor<U> HasItems<U> (Func<T, IEnumerable<U>> access, IBuilderDescriptor<U> recurse)
		{
			throw new NotImplementedException ();
		}

		public override IBuilderDescriptor<U> HasItems<U> (Func<T, IEnumerable<U>> access)
		{
			RecurseBuilderDescriptor<U, C, V>	descriptor;
			NodePointer<U, C, V>				inner;

			if (this.pointer.enter != null)
				throw new InvalidOperationException ("can't declare access method twice on same descriptor");

			descriptor = new RecurseBuilderDescriptor<U, C, V> (this.encoder);
			inner = descriptor.pointer;

			this.pointer.enter = (source, writer, context) => writer.WriteItems (access (source), inner, context);

			return descriptor;
		}

		public override void IsValue<U> (Func<T, U> access)
		{
			Converter<U, V>	convert;

			if (this.pointer.enter != null)
				throw new InvalidOperationException ("can't declare access method twice on same descriptor");

			convert = this.encoder.Get<U> ();

			this.pointer.enter = (source, writer, context) => writer.WriteValue (convert (access (source)), context);
		}
	}
}
