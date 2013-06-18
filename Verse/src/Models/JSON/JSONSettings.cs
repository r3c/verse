using System;

namespace Verse.Models.JSON
{
	[Flags]
	public enum JSONSettings
	{
		NoNullAttribute	= 0x01,
		NoNullValue		= 0x02
	}
}
