using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Reader : IReader<Context, Value>
	{
		#region Attributes

		private readonly Encoding	encoding;

		#endregion

		#region Constructors

		public Reader (Encoding encoding)
		{
			this.encoding = encoding;
		}

		#endregion

		#region Methods / Public

		public bool Begin (Stream stream, out Context context)
		{
			context = new Context (new StreamReader (stream, this.encoding));

			if (context.Current < 0 || !this.ReadBlank (context))
				return false;

			return true;
		}

		public void End (Context context)
		{
		}

		public bool Read<U> (ref U target, IPointer<U, Context, Value> builder, Context context)
		{
			StringBuilder				buffer;
			char						current;
			IPointer<U, Context, Value>	field;
			long						numberDecimal;
			int							numberDecimalPower;
			int							numberExponent;
			sbyte						numberExponentSign;
			long						numberIntegral;
			sbyte						numberSign;
			Value						value;

			switch (context.Current)
			{
				case (int)'"':
					if (!context.Next ())
						return false;

					if (builder.CanAssign)
					{
						buffer = new StringBuilder (32);

						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;

							buffer.Append (current);
						}

						context.Next ();

						builder.Assign (ref target, new Value {String = buffer.ToString (), Type = Content.String});
					}
					else
					{
						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;
						}

						context.Next ();
					}

					return true;

				case (int)'-':
				case (int)'.':
				case (int)'0':
				case (int)'1':
				case (int)'2':
				case (int)'3':
				case (int)'4':
				case (int)'5':
				case (int)'6':
				case (int)'7':
				case (int)'8':
				case (int)'9':
					unchecked
					{
						// Read sign
						if (context.Current == (int)'-')
						{
							context.Next();

							numberSign = -1;
						}
						else
							numberSign = 1;

						// Read integral part
						for (numberIntegral = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Next ())
							numberIntegral = numberIntegral * 10 + (context.Current - (int)'0');

						// Read decimal part
						if (context.Current == (int)'.')
						{
							context.Next();

							numberDecimalPower = 0;

							for (numberDecimal = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Next ())
							{
								numberDecimal = numberDecimal * 10 + (context.Current - (int)'0');
								numberDecimalPower -= 1;
							}
						}
						else
						{
							numberDecimal = 0;
							numberDecimalPower = 0;
						}

						// Read exponent
						if (context.Current == (int)'E' || context.Current == (int)'e')
						{
							context.Next ();

							switch (context.Current)
							{
								case (int)'+':
									context.Next ();

									numberExponentSign = 1;

									break;

								case (int)'-':
									context.Next ();

									numberExponentSign = -1;

									break;

								default:
									numberExponentSign = 1;

									break;
							}

							for (numberExponent = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Next ())
								numberExponent = numberExponent * 10 + (context.Current - (int)'0');

							numberExponent *= numberExponentSign;
						}
						else
							numberExponent = 0;

						if (numberDecimal != 0 || numberExponent < 0) // Decimal value with either decimal part or negative exponent
							value = new Value {Number = numberSign * (numberIntegral + numberDecimal * Math.Pow (10, numberDecimalPower)) * Math.Pow (10, numberExponent), Type = Content.Number};
						else if (numberExponent > 0) // Integer value with positive exponent
							value = new Value {Number = numberSign * numberIntegral * (long)Math.Pow (10, numberExponent), Type = Content.Number};
						else // Simple integer value
							value = new Value {Number = numberSign * numberIntegral, Type = Content.Number};
					}

					builder.Assign(ref target, value);

					return true;

				case (int)'f':
					if (!context.Next () || !context.Skip ('a') || !context.Skip ('l') || !context.Skip ('s') || context.Current != (int)'e')
						return false;

					context.Next ();

					builder.Assign (ref target, new Value {Boolean = false, Type = Content.Boolean});

					return true;

				case (int)'n':
					if (!context.Next () || !context.Skip ('u') || !context.Skip ('l') || context.Current != (int)'l')
						return false;

					context.Next ();

					builder.Assign (ref target, new Value {Type = Content.Void});

					return true;

				case (int)'t':
					if (!context.Next () || !context.Skip ('r') || !context.Skip ('u') || context.Current != (int)'e')
						return false;

					context.Next ();

					builder.Assign (ref target, new Value {Boolean = true, Type = Content.Boolean});

					return true;

				case (int)'[':
					if (!context.Next ())
						return false;

					for (int index = 0; true; ++index)
					{
						if (!this.ReadBlank (context))
							return false;

						if (context.Current == (int)']')
							break;

						if (index > 0 && (!context.Skip (',') || !this.ReadBlank (context)))
							return false;

						// Build and move to array index
						field = builder;

						if (index > 9)
						{
							foreach (char digit in index.ToString (CultureInfo.InvariantCulture))
								field = field.Next (digit);
						}
						else
							field = field.Next ((char)('0' + index));

						// Read array value
						if (!field.Enter (ref target, this, context))
							return false;
					}

					context.Next();

					return true;

				case (int)'{':
					if (!context.Next ())
						return false;

					for (int index = 0; true; ++index)
					{
						if (!this.ReadBlank (context))
							return false;

						if (context.Current == (int)'}')
							break;

						if (index > 0 && (!context.Skip (',') || !this.ReadBlank (context)))
							return false;

						if (context.Current != (int)'"' || !context.Next ())
							return false;

						// Read and move to object key
						field = builder;

						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;

							field = field.Next (current);
						}

						// Read object separator
						if (!context.Next () || !this.ReadBlank (context) || !context.Skip (':') || !this.ReadBlank (context))
							return false;

						// Read object value
						if (!field.Enter (ref target, this, context))
							return false;
					}

					context.Next ();

					return true;

				default:
					return false;
			}
		}

		#endregion

		#region Methods / Private

		private bool ReadBlank (Context source)
		{
			int current;

			while (true)
			{
				current = source.Current;

				if (current < 0)
					return false;

				if (current > (int)' ')
					return true;

				source.Next();
			}
		}

		private bool ReadCharacter (Context source, out char character)
		{
			int	nibble;
			int previous;
			int	value;

			previous = source.Current;

			source.Next();

			if (previous < 0)
			{
				character = default(char);

				return false;
			}

			if (previous != (int)'\\')
			{
				character = (char)previous;

				return true;
			}

			previous = source.Current;

			source.Next();

			switch (previous)
			{
				case -1:
					character = default(char);

					return false;

				case (int)'b':
					character = '\b';

					return true;

				case (int)'f':
					character = '\f';

					return true;

				case (int)'n':
					character = '\n';

					return true;

				case (int)'r':
					character = '\r';

					return true;

				case (int)'t':
					character = '\t';

					return true;

				case (int)'u':
					value = 0;

					for (int i = 0; i < 4; ++i)
					{
						previous = source.Current;

						source.Next();

						if (previous >= (int)'0' && previous <= (int)'9')
							nibble = previous - (int)'0';
						else if (previous >= (int)'A' && previous <= (int)'F')
							nibble = previous - (int)'A' + 10;
						else if (previous >= (int)'a' && previous <= (int)'f')
							nibble = previous - (int)'a' + 10;
						else
						{
							character = default (char);

							return false;
						}

						value = (value << 4) + nibble;
					}

					character = (char)value;

					return true;

				default:
					character = (char)previous;

					return true;
			}
		}

		#endregion
	}
}
