using System;

namespace Verse.Helpers
{
	public static class AutoBinder
	{
		#region Methods / Public
		
		public static IDecoder<T>	GetDecoder<T> (ISchema schema)
		{
			IDecoder<T>	decoder;

			decoder = schema.GetDecoder<T> ();

			AutoBinder.BindDecoder (decoder);

			return decoder;
		}

		public static IEncoder<T>	GetEncoder<T> (ISchema schema)
		{
			IEncoder<T>	encoder;

			encoder = schema.GetEncoder<T> ();

			AutoBinder.BindEncoder (encoder);

			return encoder;
		}
		
		#endregion
		
		#region Methods / Private

		private static void	BindDecoder<T> (IDecoder<T> decoder)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		private static void	BindEncoder<T> (IEncoder<T> encoder)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		#endregion
	}
}
