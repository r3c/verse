using System;

namespace Verse.EncoderDescriptors.Abstract
{
	interface IEncoderConverter<TTo>
	{
		#region Methods

		Converter<TFrom, TTo> Get<TFrom>();

		#endregion
	}
}