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

		protected abstract bool	TryLinkConvert (StringSchema.DecoderConverter<T> converter);

		protected abstract bool	TryLinkNative ();

		#endregion

		#region Methods / Protected

		protected override bool	TryLink ()
		{
        	object	converter;

        	if (this.converters.TryGetValue (typeof (T), out converter))
        		return this.TryLinkConvert ((StringSchema.DecoderConverter<T>)converter);
        	else
        		return this.TryLinkNative ();
		}

		#endregion
	}
}
