using System;

namespace Verse.DecoderDescriptors.Base
{
	interface IDecoderConverter<in TFrom>
	{
		Converter<TFrom, TTo> Get<TTo>();
	}
}