using System;
using System.Collections.Generic;

namespace Verse.Models
{
	abstract class ConvertDecoder<T, U> : AbstractDecoder<U>
	{
		#region Attributes

		protected Dictionary<Type, object>	converters;

		#endregion

		#region Constructors

		protected	ConvertDecoder (Dictionary<Type, object> converters)
		{
			this.converters = converters;
		}

		#endregion

		#region Methods / Abstract

		protected abstract bool	TryLinkConvert (ConvertSchema<T>.DecoderConverter<U> converter);

		protected abstract bool	TryLinkNative ();

		#endregion

		#region Methods / Protected

		protected override bool	TryLink ()
		{
        	object	converter;

        	if (this.converters.TryGetValue (typeof (U), out converter))
        		return this.TryLinkConvert ((ConvertSchema<T>.DecoderConverter<U>)converter);
        	else
        		return this.TryLinkNative ();
		}

		#endregion
	}
}
