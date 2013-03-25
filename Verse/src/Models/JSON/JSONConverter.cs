using System;
using System.Globalization;

namespace Verse.Models.JSON
{
	static class	JSONConverter
	{
		#region Methods

		public static void	FromBoolean (JSONPrinter printer, bool value)
		{
			printer.PrintBoolean (value);
		}

		public static void	FromChar (JSONPrinter printer, char value)
		{
			printer.PrintString (value.ToString (CultureInfo.InvariantCulture));
		}

		public static void	FromFloat4 (JSONPrinter printer, float value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromFloat8 (JSONPrinter printer, double value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromInt1s (JSONPrinter printer, sbyte value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromInt1u (JSONPrinter printer, byte value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromInt2s (JSONPrinter printer, short value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromInt2u (JSONPrinter printer, ushort value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromInt4s (JSONPrinter printer, int value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromInt4u (JSONPrinter printer, uint value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromInt8s (JSONPrinter printer, long value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromInt8u (JSONPrinter printer, ulong value)
		{
			printer.PrintNumber (value);
		}

		public static void	FromString (JSONPrinter printer, string value)
		{
			printer.PrintString (value);
		}

		public static bool	ToBoolean (JSONLexer lexer, out bool value)
		{
			switch (lexer.Lexem)
			{
				case JSONLexem.False:
					value = false;

					return true;

				case JSONLexem.True:
					value = true;

					return true;

				default:
					value = default (bool);

					return false;
			}
		}

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

			return lexer.Lexem == JSONLexem.Null || lexer.Lexem == JSONLexem.String;
		}
		
		#endregion
	}
}
