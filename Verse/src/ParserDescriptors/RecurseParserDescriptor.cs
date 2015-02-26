using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Nodes;
using Verse.Tools;

namespace Verse.ParserDescriptors
{
	class RecurseParserDescriptor<T, C, V> : AbstractParserDescriptor<T>
	{
		#region Attributes

		private readonly Container<T, C, V> container;

		private readonly IDecoder<V> decoder;

		#endregion

		#region Constructors

		public RecurseParserDescriptor (IDecoder<V> decoder)
		{
			this.container = new Container<T, C, V> ();
			this.decoder = decoder;
		}

		#endregion

		#region Methods / Public

		public IParser<T> CreateParser (IReader<C, V> reader)
		{
			return new Parser<T, C, V> (this.container, reader);
		}

		public override IParserDescriptor<U> HasField<U> (string name, ParserAssign<T, U> assign, IParserDescriptor<U> parent)
		{
			RecurseParserDescriptor<U, C, V> descriptor;

			descriptor = parent as RecurseParserDescriptor<U, C, V>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException ("parent", "invalid target descriptor type");

			return this.HasField (name, assign, descriptor);
		}

		public override IParserDescriptor<U> HasField<U> (string name, ParserAssign<T, U> assign)
		{
			return this.HasField (name, assign, new RecurseParserDescriptor<U, C, V> (this.decoder));
		}

		public override IParserDescriptor<T> HasField (string name)
		{
			RecurseParserDescriptor<T, C, V> descriptor;
			Container<T, C, V> recurse;

			descriptor = new RecurseParserDescriptor<T, C, V> (this.decoder);
			recurse = descriptor.container;

			this.Connect (name, (ref T target, IReader<C, V> reader, C context) => reader.ReadValue (ref target, recurse, context));

			return descriptor;
		}

		public override IParserDescriptor<U> IsArray<U> (ParserAssign<T, IEnumerable<U>> assign, IParserDescriptor<U> parent)
		{
			RecurseParserDescriptor<U, C, V> descriptor;

			descriptor = parent as RecurseParserDescriptor<U, C, V>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException ("parent", "incompatible descriptor type");

			return this.IsArray (assign, descriptor);
		}

		public override IParserDescriptor<U> IsArray<U> (ParserAssign<T, IEnumerable<U>> assign)
		{
			return this.IsArray (assign, new RecurseParserDescriptor<U, C, V> (this.decoder));
		}

		public override void IsValue<U> (ParserAssign<T, U> assign)
		{
			Converter<V, U> convert;

			convert = this.decoder.Get<U> ();

			this.container.value = (ref T target, V value) => assign (ref target, convert (value));
		}

		#endregion

		#region Methods / Private

		private void Connect (string name, Follow<T, C, V> enter)
		{
			BranchNode<T, C, V> next;

			next = this.container.fields;

			foreach (char c in name)
				next = next.Connect (c);

			next.enter = enter;
		}

		private IParserDescriptor<U> HasField<U> (string name, ParserAssign<T, U> assign, RecurseParserDescriptor<U, C, V> descriptor)
		{
			Func<T, U> constructor;
			Container<U, C, V> recurse;

			constructor = this.GetConstructor<U> ();
			recurse = descriptor.container;

			this.Connect (name, (ref T target, IReader<C, V> reader, C context) =>
			{
				U	inner;

				inner = constructor (target);

				if (!reader.ReadValue (ref inner, recurse, context))
					return false;

				assign (ref target, inner);

				return true;
			});

			return descriptor;
		}

		private IParserDescriptor<U> IsArray<U> (ParserAssign<T, IEnumerable<U>> assign, RecurseParserDescriptor<U, C, V> descriptor)
		{
			Func<T, U> constructor;
			Container<U, C, V> recurse;

			if (this.container.items != null)
				throw new InvalidOperationException ("can't declare items twice on same descriptor");

			constructor = this.GetConstructor<U> ();
			recurse = descriptor.container;

			this.container.items = (ref T target, IReader<C, V> reader, C context) =>
			{
				IBrowser<U>	browser;
				T			source;

				source = target;
				browser = reader.ReadArray (() => constructor (source), recurse, context);
				assign (ref target, new Walker<U> (browser));

				while (browser.MoveNext ())
					;

				return browser.Success;
			};

			return descriptor;
		}

		#endregion
	}
}
