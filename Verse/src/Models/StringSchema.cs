using System;
using System.Collections.Generic;

namespace Verse.Models
{
	public abstract class StringSchema : ISchema
	{
		#region Attributes

		protected Dictionary<Type, object>	decoderConverters;

		#endregion
		
		#region Constructors
		
		protected	StringSchema ()
		{
			this.decoderConverters = new Dictionary<Type, object> ();
		}
		
		#endregion
		
		#region Methods / Abstract

		public abstract IDecoder<T>	GetDecoder<T> (Func<T> constructor);

		public abstract IEncoder<T>	GetEncoder<T> ();
		
		#endregion
		
		#region Methods / Public
		
		public void	SetDecoderConverter<T> (DecoderConverter<T> converter)
		{
			this.decoderConverters[typeof (T)] = converter;
		}

		public void	SetEncoderConverter<T> (EncoderConverter<T> converter)
		{
			throw new NotImplementedException ();
		}

		#endregion
		
		#region Types
		
		public delegate bool	DecoderConverter<T> (string input, out T value);
		
		public delegate bool	EncoderConverter<T> (T input, out string value);
		
		#endregion
	}
}
