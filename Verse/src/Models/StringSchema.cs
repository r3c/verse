using System;
using System.Collections.Generic;

namespace Verse.Models
{
	public abstract class StringSchema : AbstractSchema
	{
		#region Attributes

		protected Dictionary<Type, object>	decoderConverters;

		protected Dictionary<Type, object>	encoderConverters;

		#endregion
		
		#region Constructors
		
		protected	StringSchema ()
		{
			this.decoderConverters = new Dictionary<Type, object> ();
			this.encoderConverters = new Dictionary<Type, object> ();
		}
		
		#endregion
		
		#region Methods / Public
		
		public void	SetDecoderConverter<T> (DecoderConverter<T> converter)
		{
			this.decoderConverters[typeof (T)] = converter;
		}

		public void	SetEncoderConverter<T> (EncoderConverter<T> converter)
		{
			this.encoderConverters[typeof (T)] = converter;
		}

		#endregion
		
		#region Types
		
		public delegate bool	DecoderConverter<T> (string input, out T value);
		
		public delegate string	EncoderConverter<T> (T input);
		
		#endregion
	}
}
