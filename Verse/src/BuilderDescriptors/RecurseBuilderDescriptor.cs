using System;
using System.Collections.Generic;
using Verse.BuilderDescriptors.Recurse;

namespace Verse.BuilderDescriptors
{
	class RecurseBuilderDescriptor<T, C, V> : AbstractBuilderDescriptor<T>
	{
		#region Attributes

		private readonly Container<T, C, V>	container;

		private readonly IEncoder<V>		encoder;

		#endregion

		#region Constructors

		public RecurseBuilderDescriptor (IEncoder<V> encoder)
		{
			this.container = new Container<T, C, V> ();
			this.encoder = encoder;
		}

		#endregion

		#region Methods

		public IBuilder<T> GetBuilder (IWriter<C, V> writer)
		{
			return new Builder<T, C, V> (this.container, writer);
		}

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

			convert = this.encoder.Get<U> ();

			this.container.value = (source) => convert (access (source));
		}

		#endregion

		#region Methods / Private

		private RecurseBuilderDescriptor<U, C, V> HasField<U> (string name, Func<T, U> access, RecurseBuilderDescriptor<U, C, V> descriptor)
		{
			Container<U, C, V>	recurse;

			recurse = descriptor.container;

			this.container.fields[name] = (source, writer, context) => writer.Write (access (source), recurse, context);

			return descriptor;
		}

		private RecurseBuilderDescriptor<U, C, V> HasItems<U> (Func<T, IEnumerable<U>> access, RecurseBuilderDescriptor<U, C, V> descriptor)
		{
			Container<U, C, V>	recurse;

			if (this.container.items != null)
				throw new InvalidOperationException ("can't declare items twice on same descriptor");

			recurse = descriptor.container;
			
			this.container.items = (source, writer, context) => writer.WriteItems (access (source), recurse, context);

			return descriptor;
		}

		#endregion
	}
}
