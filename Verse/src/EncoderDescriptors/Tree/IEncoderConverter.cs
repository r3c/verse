using System;

namespace Verse.EncoderDescriptors.Tree
{
	interface IEncoderConverter<out TTo>
	{
		Converter<TFrom, TTo> Get<TFrom>();
	}
}