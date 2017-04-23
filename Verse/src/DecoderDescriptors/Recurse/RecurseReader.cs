using System;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.DecoderDescriptors.Recurse
{
	abstract class RecurseReader<TEntity, TState, TValue> : IReader<TEntity, TState>
	{
		#region Properties

		protected bool IsArray
		{
			get
			{
				return this.array != null;
			}
		}

		protected bool IsValue
		{
			get
			{
				return this.convert != null;
			}
		}

		#endregion

		#region Attributes

		private EntityReader<TEntity, TState> array = null;

		private Converter<TValue, TEntity> convert = null;

		#endregion

		#region Methods / Abstract

		public abstract BrowserMove<TEntity> Browse(Func<TEntity> constructor, TState state);

		public abstract RecurseReader<TOther, TState, TValue> Create<TOther>();

		public abstract void DeclareField(string name, EntityReader<TEntity, TState> enter);

		public abstract bool Read(ref TEntity entity, TState state);

		#endregion

		#region Methods / Public

		public void DeclareArray(EntityReader<TEntity, TState> enter)
		{
			if (this.array != null)
				throw new InvalidOperationException("can't declare array twice on same descriptor");

			this.array = enter;
		}

		public void DeclareValue(Converter<TValue, TEntity> convert)
		{
			if (this.convert != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.convert = convert;
		}

		#endregion

		#region Methods / Protected

		protected TEntity ConvertValue(TValue value)
		{
			return this.convert(value);
		}

		protected bool ReadArray(ref TEntity entity, TState state)
		{
			return this.array(ref entity, state);
		}

		#endregion
	}
}