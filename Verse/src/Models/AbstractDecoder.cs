using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Models
{
	abstract class	Decoder<T> : IDecoder<T>
	{
		#region Methods / Abstract

		public abstract bool		Decode (Stream stream, out T instance);

		public abstract IDecoder<U>	Define<U> (string name, Func<U> builder, DecoderFieldSetter<T, U> setter);

		public abstract IDecoder<U>	Define<U> (Func<U> builder, DecoderSubSetter<T, U> setter);

		public abstract void		Link (Func<T> builder);
		
		public abstract void		Link ();

		#endregion

		#region Methods / Public

		public IDecoder<U>	Define<U> (string name, DecoderFieldSetter<T, U> setter)
		{
			Func<U>	constructor;
			
			constructor = this.GetConstructor<U> ();

			return this.Define<U> (name, () => constructor (), setter);
		}

		public IDecoder<U>	Define<U> (DecoderSubSetter<T, U> setter)
		{
			Func<U>	constructor;
			
			constructor = this.GetConstructor<U> ();

			return this.Define<U> (() => constructor (), setter);
		}

		#endregion

		#region Methods / Private

		private Func<U>	GetConstructor<U> ()
	    {
	    	ILGenerator		generator;
	    	ConstructorInfo	info;
	        DynamicMethod	method;
	        Type			type;

        	type = typeof (U);
			info = type.GetConstructor (Type.EmptyTypes);

			if (info == null)
				return () => default (U);

			method = new DynamicMethod (string.Empty, type, Type.EmptyTypes, info.DeclaringType);

	        generator = method.GetILGenerator ();
	        generator.Emit (OpCodes.Newobj, info);
	        generator.Emit (OpCodes.Ret);

	        return (Func<U>)method.CreateDelegate (typeof (Func<U>));
	    }

	    #endregion
	}
}
