using System;
using System.Collections.Generic;
using Verse.BuilderDescriptors.Recurse;

namespace Verse.BuilderDescriptors
{
	class RecurseBuilderDescriptor<T, C, V> : AbstractBuilderDescriptor<T>
	{
		#region Properties

		public Pointer<T, C, V>	Pointer
		{
			get
			{
				return this.pointer;
			}
		}

		#endregion

		#region Attributes

		private readonly IEncoder<V>		encoder;

		private readonly Pointer<T, C, V>	pointer;

		#endregion

		#region Constructors

		public RecurseBuilderDescriptor (IEncoder<V> encoder)
		{
			this.encoder = encoder;
			this.pointer = new Pointer<T, C, V> ();
		}

		#endregion

		#region Methods

		public override IBuilderDescriptor<U> HasField<U> (string name, Func<T, U> access, IBuilderDescriptor<U> parent)
		{
			RecurseBuilderDescriptor<U, C, V>	descriptor;

			descriptor = parent as RecurseBuilderDescriptor<U, C, V>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException ("parent", "incompatible descriptor type");

			return this.HasField (name, access, descriptor);
		}

		public override IBuilderDescriptor<U> HasField<U> (string name, Func<T, U> access)
		{
			return this.HasField (name, access, new RecurseBuilderDescriptor<U, C, V> (this.encoder));
		}

		public override IBuilderDescriptor<U> HasItems<U> (Func<T, IEnumerable<U>> access, IBuilderDescriptor<U> parent)
		{
			RecurseBuilderDescriptor<U, C, V>	descriptor;

			descriptor = parent as RecurseBuilderDescriptor<U, C, V>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException ("parent", "incompatible descriptor type");

			return this.HasItems (access, descriptor);
		}

		public override IBuilderDescriptor<U> HasItems<U> (Func<T, IEnumerable<U>> access)
		{
			return this.HasItems (access, new RecurseBuilderDescriptor<U, C, V> (this.encoder));
		}

		public override void IsValue<U> (Func<T, U> access)
		{
			Converter<U, V>	convert;

			if (this.pointer.value != null)
				throw new InvalidOperationException ("can't declare value twice on same descriptor");

			convert = this.encoder.Get<U> ();

			this.pointer.value = (source) => convert (access (source));
		}

		#endregion

		#region Methods / Private

		private RecurseBuilderDescriptor<U, C, V> HasField<U> (string name, Func<T, U> access, RecurseBuilderDescriptor<U, C, V> descriptor)
		{
			Pointer<U, C, V>	recurse;

			recurse = descriptor.pointer;

			this.pointer.fields[name] = (source, writer, context) => writer.Write (access (source), recurse, context);

			return descriptor;
		}

		private RecurseBuilderDescriptor<U, C, V> HasItems<U> (Func<T, IEnumerable<U>> access, RecurseBuilderDescriptor<U, C, V> descriptor)
		{
			Pointer<U, C, V>	recurse;

			if (this.pointer.items != null)
				throw new InvalidOperationException ("can't declare items twice on same descriptor");

			recurse = descriptor.pointer;
			
			this.pointer.items = (source, writer, context) =>
			{
				foreach (U item in access (source))
					writer.Write (item, recurse, context);
			};

			return descriptor;
		}

		#endregion
	}
}
