using System;
using System.Collections.Generic;
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

		public abstract IParserDescriptor<U> HasItems<U> (ParserAssign<T, IEnumerable<U>> assign, IParserDescriptor<U> parent);

		public abstract IParserDescriptor<U> HasItems<U> (ParserAssign<T, IEnumerable<U>> assign);

		public abstract void IsValue<U> (ParserAssign<T, U> assign);

		#endregion

		#region Methods / Public

		public void CanCreate<U> (Func<T, U> constructor)
		{
			if (constructor == null)
				throw new ArgumentNullException ("constructor");

			this.constructors[typeof (U)] = constructor;
		}
/*
		public IParserDescriptor<T> HasKey (string name)
		{
			return this.HasKey (name, (ref T target, T value) => {});
		}
*/
		public void IsValue ()
		{
			this.IsValue ((ref T target, T value) => target = value);
		}

		#endregion

		#region Methods / Protected

		protected Func<T, U> GetConstructor<U> ()
		{
			object	box;
			Func<U>	constructor;

			if (!this.constructors.TryGetValue (typeof (T), out box))
			{
				constructor = Generator.Constructor<U> ();

				return (source) => constructor ();
			}

			return (Func<T, U>)box;
		}

		#endregion
	}
}
