using System;
using System.Collections.Generic;
using System.Reflection;

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
			MemberInfo[]	members;
			Type			type;

			type = typeof (T);

			if (type.IsSubclassOf (typeof (IEnumerable<>)))
			{
				encoder.
			}

			members = typeof (T).GetMembers (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			if (members.Length == 0)
			{
				encoder.Bind ();

				return;
			}

			foreach (MemberInfo member in members)
				encoder.HasField (member.Name, null); // FIXME

			// FIXME
			throw new NotImplementedException ();
		}

		#endregion
	}
}
