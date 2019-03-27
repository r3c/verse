using System;

namespace Verse.EncoderDescriptors.Abstract
{
	interface IEncoderConverter<out TTo>
	{
		Converter<TFrom, TTo> Get<TFrom>();
	}
}