using System;
using System.Collections.Generic;

namespace Verse.BuilderDescriptors
{
	abstract class AbstractBuilderDescriptor<T> : IBuilderDescriptor<T>
	{
		#region Methods / Abstract

		public abstract IBuilderDescriptor<U> HasField<U> (string name, Func<T, U> access, IBuilderDescriptor<U> parent);

		public abstract IBuilderDescriptor<U> HasField<U> (string name, Func<T, U> access);		

		public abstract IBuilderDescriptor<U> HasItems<U> (Func<T, IEnumerable<U>> access, IBuilderDescriptor<U> parent);
		
		public abstract IBuilderDescriptor<U> HasItems<U> (Func<T, IEnumerable<U>> access);

		public abstract void IsValue<U> (Func<T, U> access);

		#endregion

		#region Methods / Public

		public IBuilderDescriptor<T> HasField (string name)
		{
			return this.HasField (name, (source) => source); 
		}

		public void IsValue ()
		{
			this.IsValue ((target) => target);
		}

		#endregion
	}
}
