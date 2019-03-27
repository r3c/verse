using System;

namespace Verse.DecoderDescriptors.Abstract
{
	interface IDecoderConverter<in TFrom>
	{
		Converter<TFrom, TTo> Get<TTo>();
	}
}