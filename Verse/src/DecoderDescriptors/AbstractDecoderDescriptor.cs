using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Abstract;
using Verse.Tools;

namespace Verse.DecoderDescriptors
{
	abstract class AbstractDecoderDescriptor<TEntity, TValue> : IDecoderDescriptor<TEntity>
	{
		#region Attributes

		private readonly Dictionary<Type, object> constructors;

		protected readonly IDecoderConverter<TValue> converter;

		#endregion

		#region Constructors

		protected AbstractDecoderDescriptor(IDecoderConverter<TValue> converter)
		{
			this.constructors = new Dictionary<Type, object>();
			this.converter = converter;
		}

		#endregion

		#region Methods / Abstract

		public abstract IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent);

		public abstract IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign);

		public abstract IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, IDecoderDescriptor<TElement> parent);

		public abstract IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign);

		public abstract void IsValue<TCompatible>(DecodeAssign<TEntity, TCompatible> assign);

		#endregion

		#region Methods / Public

		public void CanCreate<TField>(Func<TField> constructor)
		{
			if (constructor == null)
				throw new ArgumentNullException("constructor");

			this.constructors[typeof (TField)] = constructor;
		}

		public void IsValue()
		{
			this.IsValue((ref TEntity target, TEntity value) => target = value);
		}

		#endregion

		#region Methods / Protected

		protected Func<TField> GetConstructor<TField>()
		{
			object constructor;

			if (this.constructors.TryGetValue(typeof (TField), out constructor))
				return (Func<TField>)constructor;

			return Generator.Constructor<TField>();
		}

		protected Converter<TValue, TRaw> GetConverter<TRaw>()
		{
			return this.converter.Get<TRaw>();
		}

		#endregion
	}
}