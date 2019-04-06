using System;

namespace Verse.EncoderDescriptors.Base
{
	interface IEncoderConverter<out TTo>
	{
		Converter<TFrom, TTo> Get<TFrom>();
	}
}