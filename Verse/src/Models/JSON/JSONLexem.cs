using System;

namespace Verse.Models.JSON
{
	enum JSONLexem
	{
		Unknown,
		ArrayBegin,
		ArrayEnd,
		Colon,
		Comma,
		False,
		Null,
		Number,
		ObjectBegin,
		ObjectEnd,
		String,
		True
	}
}
