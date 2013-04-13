using System;
using System.Collections.Generic;

namespace Verse.Models
{
	abstract class ConvertEncoder<T, U> : AbstractEncoder<U>
	{
		#region Attributes

		protected Dictionary<Type, object>	converters;

		#endregion

		#region Constructors

		protected	ConvertEncoder (Dictionary<Type, object> converters)
		{
			this.converters = converters;
		}

		#endregion

		#region Methods / Abstract

		protected abstract bool	TryLinkConvert (ConvertSchema<T>.EncoderConverter<U> converter);

		protected abstract bool	TryLinkNative ();

		#endregion

		#region Methods / Protected

		protected override bool	TryLink ()
		{
        	object	converter;

        	if (this.converters.TryGetValue (typeof (U), out converter))
        		return this.TryLinkConvert ((ConvertSchema<T>.EncoderConverter<U>)converter);
        	else
        		return this.TryLinkNative ();
		}

		#endregion
	}
}
