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

		public abstract IDecoder<U>	Field<U> (string name, Func<U> builder, DecoderValueSetter<T, U> setter);

		public abstract IDecoder<U>	Field<U> (Func<U> builder, DecoderKeyValueSetter<T, U> setter);

		public abstract IDecoder<U>	Field<U> (Func<U> builder, DecoderValueSetter<T, U> setter);

		#endregion

		#region Methods / Public

		public IDecoder<U>	Field<U> (string name, DecoderValueSetter<T, U> setter)
		{
			Func<U>	constructor;
			
			constructor = this.GetConstructor<U> ();

			return this.Field<U> (name, () => constructor (), setter);
		}

		public IDecoder<U>	Field<U> (DecoderKeyValueSetter<T, U> setter)
		{
			Func<U>	constructor;
			
			constructor = this.GetConstructor<U> ();

			return this.Field<U> (() => constructor (), setter);
		}

		public IDecoder<U>	Field<U> (DecoderValueSetter<T, U> setter)
		{
			Func<U>	constructor;
			
			constructor = this.GetConstructor<U> ();

			return this.Field<U> (() => constructor (), setter);
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
