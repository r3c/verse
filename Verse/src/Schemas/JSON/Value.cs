using System;
using System.Runtime.InteropServices;

namespace Verse.Schemas.JSON
{
	[StructLayout (LayoutKind.Explicit)]
	public struct Value
	{
		[FieldOffset (sizeof (Content))]
		public bool		Boolean;

		[FieldOffset (sizeof (Content))]
		public double	Number;

		[FieldOffset (sizeof (Content) + sizeof (double))]
		public string	String;

		[FieldOffset (0)]
		public Content	Type;
	}
}
