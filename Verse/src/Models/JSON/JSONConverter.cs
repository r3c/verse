using System;
using System.Collections.Generic;

namespace Verse.Models.JSON
{
	static class JSONConverter
	{
		#region Attributes
		
		private static readonly Dictionary<Type, object>	extractors = new Dictionary<Type, object>
		{
			{typeof (bool),		(JSONExtractor<bool>)((JSONLexer lexer, out bool value) => { value = lexer.Lexem != JSONLexem.False; return lexer.Lexem == JSONLexem.False || lexer.Lexem == JSONLexem.True; })},
			{typeof (char),		(JSONExtractor<char>)((JSONLexer lexer, out char value) => { value = lexer.AsString.Length > 0 ? lexer.AsString[0] : '\0'; return lexer.Lexem == JSONLexem.String; })},
			{typeof (float),	(JSONExtractor<float>)((JSONLexer lexer, out float value) => { value = (float)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (double),	(JSONExtractor<double>)((JSONLexer lexer, out double value) => { value = lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (sbyte),	(JSONExtractor<sbyte>)((JSONLexer lexer, out sbyte value) => { value = (sbyte)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (byte),		(JSONExtractor<byte>)((JSONLexer lexer, out byte value) => { value = (byte)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (short),	(JSONExtractor<short>)((JSONLexer lexer, out short value) => { value = (short)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (ushort),	(JSONExtractor<ushort>)((JSONLexer lexer, out ushort value) => { value = (ushort)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (int),		(JSONExtractor<int>)((JSONLexer lexer, out int value) => { value = (int)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (uint),		(JSONExtractor<uint>)((JSONLexer lexer, out uint value) => { value = (uint)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (long),		(JSONExtractor<long>)((JSONLexer lexer, out long value) => { value = (long)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (ulong),	(JSONExtractor<ulong>)((JSONLexer lexer, out ulong value) => { value = (ulong)lexer.AsDouble; return lexer.Lexem == JSONLexem.Number; })},
			{typeof (string),	(JSONExtractor<string>)((JSONLexer lexer, out string value) => { value = lexer.AsString; return lexer.Lexem == JSONLexem.String; })},
		};

		private static readonly Dictionary<Type, object>	injectors = new Dictionary<Type, object>
		{
			{typeof (bool),		(JSONInjector<bool>)((printer, value) => { printer.WriteBoolean (value); return true; })},
			{typeof (char),		(JSONInjector<char>)((printer, value) => { printer.WriteString (new string (value, 1)); return true; })},
			{typeof (float),	(JSONInjector<float>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (double),	(JSONInjector<double>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (sbyte),	(JSONInjector<sbyte>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (byte),		(JSONInjector<byte>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (short),	(JSONInjector<short>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (ushort),	(JSONInjector<ushort>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (int),		(JSONInjector<int>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (uint),		(JSONInjector<uint>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (long),		(JSONInjector<long>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (ulong),	(JSONInjector<ulong>)((printer, value) => { printer.WriteNumber (value); return true; })},
			{typeof (string),	(JSONInjector<string>)((printer, value) => { printer.WriteString (value); return true; })},
		};

		#endregion

		#region Methods

		public static bool	TryGetExtractor (Type type, out object extractor)
		{
			return JSONConverter.extractors.TryGetValue (type, out extractor);
		}

		public static bool	TryGetInjector (Type type, out object injector)
		{
			return JSONConverter.injectors.TryGetValue (type, out injector);
		}

		#endregion
	}
}
