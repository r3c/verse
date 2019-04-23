using System;

namespace Verse.DecoderDescriptors.Tree
{
	internal interface IDecoderConverter<in TFrom>
	{
		Converter<TFrom, TTo> Get<TTo>();
	}
}