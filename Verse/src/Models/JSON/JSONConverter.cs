using System;

namespace Verse.Models.JSON
{
	static class	JSONConverter
	{
		#region Methods
		
		public static bool	ToChar (JSONLexer lexer, out char value)
        {
        	value = (char)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToFloat4 (JSONLexer lexer, out float value)
        {
        	value = (float)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToFloat8 (JSONLexer lexer, out double value)
        {
        	value = lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToInt1s (JSONLexer lexer, out sbyte value)
        {
        	value = (sbyte)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToInt1u (JSONLexer lexer, out byte value)
        {
        	value = (byte)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToInt2s (JSONLexer lexer, out short value)
        {
        	value = (short)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToInt2u (JSONLexer lexer, out ushort value)
        {
        	value = (ushort)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToInt4s (JSONLexer lexer, out int value)
        {
        	value = (int)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToInt4u (JSONLexer lexer, out uint value)
        {
        	value = (uint)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToInt8s (JSONLexer lexer, out long value)
        {
        	value = (long)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToInt8u (JSONLexer lexer, out ulong value)
        {
        	value = (ulong)lexer.AsDouble;

			return lexer.Lexem == JSONLexem.Number;
		}

        public static bool	ToString (JSONLexer lexer, out string value)
        {
        	value = lexer.AsString;

			return lexer.Lexem == JSONLexem.String;
		}
		
		#endregion
	}
}
