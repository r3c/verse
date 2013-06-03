using System;
using System.Collections.Generic;

namespace Verse.Models
{
	public abstract class ConvertSchema<T> : AbstractSchema
	{
		#region Attributes

		protected Dictionary<Type, object>	decoderConverters;

		protected Dictionary<Type, object>	encoderConverters;

		#endregion
		
		#region Constructors
		
		protected	ConvertSchema ()
		{
			this.decoderConverters = new Dictionary<Type, object> ();
			this.encoderConverters = new Dictionary<Type, object> ();
		}
		
		#endregion
		
		#region Methods
		
		public void	SetDecoderConverter<U> (DecoderConverter<U> converter)
		{
			this.decoderConverters[typeof (U)] = converter;
		}

		public void	SetDecoderConverter<U> (Converter<T, U> converter)
		{
			this.SetDecoderConverter ((T input, out U output) =>
			{
				output = converter (input);

				return true;
			});
		}

		public void	SetEncoderConverter<U> (EncoderConverter<U> converter)
		{
			this.encoderConverters[typeof (U)] = converter;
		}

		public void	SetEncoderConverter<U> (Converter<U, T> converter)
		{
			this.SetEncoderConverter ((U input, out T output) =>
			{
				output = converter (input);

				return true;
			});
		}

		#endregion
		
		#region Types
		
		public delegate bool	DecoderConverter<U> (T input, out U value);
		
		public delegate bool	EncoderConverter<U> (U input, out T value);
		
		#endregion
	}
}
