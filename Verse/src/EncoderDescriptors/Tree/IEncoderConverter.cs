using System;

namespace Verse.EncoderDescriptors.Tree
{
	internal interface IEncoderConverter<out TTo>
	{
		Converter<TFrom, TTo> Get<TFrom>();
	}
}
