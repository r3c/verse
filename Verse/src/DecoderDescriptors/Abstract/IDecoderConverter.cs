using System;

namespace Verse.DecoderDescriptors.Abstract
{
	interface IDecoderConverter<TFrom>
	{
		#region Methods

		Converter<TFrom, TTo> Get<TTo>();

		#endregion
	}
}