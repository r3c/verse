using System;

namespace Verse.DecoderDescriptors.Tree
{
	interface IDecoderConverter<in TFrom>
	{
		Converter<TFrom, TTo> Get<TTo>();
	}
}