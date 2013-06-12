using System;

namespace Verse.Models.JSON
{
	delegate bool JSONExtractor<T> (JSONLexer lexer, out T value);
}
