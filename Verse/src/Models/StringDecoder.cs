using System;
using System.Collections.Generic;

namespace Verse.Models
{
	abstract class StringDecoder<T> : AbstractDecoder<T>
	{
		#region Attributes

		protected Dictionary<Type, object>	converters;

		#endregion

		#region Constructors

		protected	StringDecoder (Dictionary<Type, object> converters)
		{
			this.converters = converters;
		}

		#endregion

		#region Methods / Abstract

		protected abstract void	BindConvert (StringSchema.DecoderConverter<T> converter);

		protected abstract void	BindNative ();

		#endregion

		#region Methods / Public

		public override void	Bind ()
		{
        	object	converter;

        	if (this.converters.TryGetValue (typeof (T), out converter))
        		this.BindConvert ((StringSchema.DecoderConverter<T>)converter);
        	else
        		this.BindNative ();
		}

		#endregion
	}
}
