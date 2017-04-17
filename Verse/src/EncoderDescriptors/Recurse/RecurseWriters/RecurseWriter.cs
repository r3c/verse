using System;
using System.Collections.Generic;

namespace Verse.EncoderDescriptors.Recurse.RecurseWriters
{
	abstract class RecurseWriter<TEntity, TState, TValue> : IRecurseWriter<TEntity, TState, TValue>
	{
		#region Properties

		public bool IsArray
		{
			get
			{
				return this.array != null;
			}
		}

		public bool IsValue
		{
			get
			{
				return this.value != null;
			}
		}

		#endregion

		#region Attributes

		private WriteEntity<TEntity, TState> array = null;

		private Converter<TEntity, TValue> value = null;

		#endregion

		#region Methods / Abstract

		public abstract IRecurseWriter<TOther, TState, TValue> Create<TOther>();

		public abstract void DeclareField(string name, WriteEntity<TEntity, TState> enter);

		public abstract void WriteElements(IEnumerable<TEntity> elements, TState state);

		public abstract void WriteEntity(TEntity source, TState state);

		#endregion

		#region Methods / Public

		public void DeclareArray(WriteEntity<TEntity, TState> enter)
		{
			if (this.array != null)
				throw new InvalidOperationException("can't declare array twice on same descriptor");

			this.array = enter;            
		}

		public void DeclareValue(Converter<TEntity, TValue> converter)
		{
			if (this.value != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.value = converter;
		}

		#endregion

		#region Methods / Protected

		protected void ProcessArray(TEntity entity, TState state)
		{
			if (this.array == null)
				throw new InvalidOperationException("internal error, cannot process undeclared array");

			this.array(entity, state);
		}

		protected TValue ProcessValue(TEntity entity)
		{
			if (this.value == null)
				throw new InvalidOperationException("internal error, cannot process undeclared value");

			return this.value(entity);
		}

		#endregion
	}
}
