using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using Verse.Events;

namespace Verse.Models
{
	abstract class	AbstractDecoder<T> : IDecoder<T>
	{
		#region Events
		
		public event StreamErrorEvent	OnStreamError;

		public event TypeErrorEvent		OnTypeError;
		
		#endregion

		#region Methods / Abstract

		public abstract void		Bind (Func<T> builder);
		
		public abstract void		Bind ();

		public abstract bool		Decode (Stream stream, out T instance);

		public abstract IDecoder<U>	HasField<U> (string name, Func<U> builder, DecoderValueSetter<T, U> setter);

		public abstract IDecoder<U>	HasItems<U> (Func<U> builder, DecoderArraySetter<T, U> setter);

		public abstract IDecoder<U>	HasPairs<U> (Func<U> builder, DecoderMapSetter<T, U> setter);

		#endregion

		#region Methods / Public

		public IDecoder<U>	HasField<U> (string name, DecoderValueSetter<T, U> setter)
		{
			return this.HasField<U> (name, this.GetConstructor<U> (), setter);
		}

		public IDecoder<U>	HasItems<U> (DecoderArraySetter<T, U> setter)
		{
			return this.HasItems<U> (this.GetConstructor<U> (), setter);
		}

		public IDecoder<U>	HasPairs<U> (DecoderMapSetter<T, U> setter)
		{
			return this.HasPairs<U> (this.GetConstructor<U> (), setter);
		}

		#endregion
		
		#region Methods / Protected

		protected void	EventStreamError (long position, string message)
		{
			StreamErrorEvent	error;

			error = this.OnStreamError;

			if (error != null)
				error (position, message);
		}

		protected void	EventTypeError (Type type, string value)
		{
			TypeErrorEvent	error;

			error = this.OnTypeError;

			if (error != null)
				error (type, value);
		}

		#endregion

		#region Methods / Private

		private Func<U>	GetConstructor<U> ()
	    {
	    	ConstructorInfo	constructor;
	    	ILGenerator		generator;
	        DynamicMethod	method;
	        Type			type;

        	type = typeof (U);
			constructor = type.GetConstructor (Type.EmptyTypes);

			if (constructor == null)
				return () => default (U);

			method = new DynamicMethod (string.Empty, type, Type.EmptyTypes, constructor.DeclaringType);

	        generator = method.GetILGenerator ();
	        generator.Emit (OpCodes.Newobj, constructor);
	        generator.Emit (OpCodes.Ret);

	        return (Func<U>)method.CreateDelegate (typeof (Func<U>));
	    }

	    #endregion
	}
}
