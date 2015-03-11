using System;
using System.Collections.Generic;
using System.Reflection;
using Verse.Tools;

namespace Verse.ParserDescriptors
{
	abstract class AbstractParserDescriptor<T> : IParserDescriptor<T>
	{
		#region Attributes

		private readonly Dictionary<Type, object>	constructors = new Dictionary<Type, object> ();

		#endregion

		#region Methods / Abstract

		public abstract IParserDescriptor<U> HasField<U> (string name, ParserAssign<T, U> assign, IParserDescriptor<U> parent);

		public abstract IParserDescriptor<U> HasField<U> (string name, ParserAssign<T, U> assign);

		public abstract IParserDescriptor<T> HasField (string name);

		public abstract IParserDescriptor<U> IsArray<U> (ParserAssign<T, IEnumerable<U>> assign, IParserDescriptor<U> parent);

		public abstract IParserDescriptor<U> IsArray<U> (ParserAssign<T, IEnumerable<U>> assign);

		public abstract void IsValue<U> (ParserAssign<T, U> assign);

		#endregion

		#region Methods / Public

		public void CanCreate<U> (Func<T, U> constructor)
		{
			if (constructor == null)
				throw new ArgumentNullException ("constructor");

			this.constructors[typeof (U)] = constructor;
		}

		public void IsValue ()
		{
			this.IsValue ((ref T target, T value) => target = value);
		}

		#endregion

		#region Methods / Protected

		protected Func<T, U> GetConstructor<U> ()
		{
			object box;
			Func<U> constructor;

			if (!this.constructors.TryGetValue (typeof (U), out box))
			{
				constructor = Generator.Constructor<U> ();

				return (source) => constructor ();
			}

			return (Func<T, U>)box;
		}

		#endregion
	}
}
