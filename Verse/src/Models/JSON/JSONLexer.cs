using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Models.JSON
{
	class JSONLexer : IDisposable
	{
		#region Properties
		
		public double		AsDouble
		{
			get
			{
				return this.asDouble;
			}
		}

		public string		AsString
		{
			get
			{
				return this.asString;
			}
		}

		public JSONLexem	Lexem
		{
			get
			{
				return this.lexem;
			}
		}
		
		#endregion
		
		#region Attributes
		
		private double		asDouble;

		private string		asString;

		private int			current;

		private JSONLexem	lexem;

		private TextReader  reader;
		
		#endregion
		
		#region Constructors

		public  JSONLexer (Stream stream, Encoding encoding)
		{
			TextReader	reader;
			
			reader = new StreamReader (stream, encoding);

			this.asDouble = 0;
			this.asString = string.Empty;
			this.current = reader.Read ();
			this.lexem = JSONLexem.Unknown;
			this.reader = reader;
		}
		
		#endregion
		
		#region Methods
		
		public void Dispose ()
		{
			this.reader.Dispose ();
		}

		public bool	Next ()
		{
			StringBuilder	buffer;

			while (this.current >= 0 && this.current <= (int)' ')
				this.current = this.reader.Read ();

			if (this.current == -1)
			{
				this.lexem = JSONLexem.Unknown;
			
				return false;
			}

			switch ((char)this.current)
			{
				case '"':
					buffer = new StringBuilder (64);

					for (this.current = this.reader.Read (); this.current != -1 && this.current != (char)'"'; )
					{
						if (this.current == '\\')
						{
							this.current = this.reader.Read ();

							if (this.current == -1)
								break;

							switch ((char)this.current)
							{
								case 'b':
									buffer.Append ('\b');

									break;

								case 'f':
									buffer.Append ('\f');

									break;

								case 'n':
									buffer.Append ('\n');

									break;

								case 'r':
									buffer.Append ('\r');

									break;

								case 't':
									buffer.Append ('\t');

									break;

								case 'u':
									throw new NotImplementedException ();

								default:
									buffer.Append ((char)this.current);

									break;
							}
						}
						else
							buffer.Append ((char)this.current);

						this.current = this.reader.Read ();
					}
					
					if (this.current == (char)'"')
					{
						this.asString = buffer.ToString ();
						this.current = this.reader.Read ();
						this.lexem = JSONLexem.String;
					}
					else
						this.lexem = JSONLexem.Unknown;

					break;

				case ',':
					this.current = this.reader.Read ();
					this.lexem = JSONLexem.Comma;
					
					break;

				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					buffer = new StringBuilder (16);

					while ((this.current >= (int)'0' && this.current <= (int)'9') ||
					       (this.current == (int)'.'))
					{
						buffer.Append ((char)this.current);

						this.current = this.reader.Read ();
					}

					if (double.TryParse (buffer.ToString (), NumberStyles.Float, CultureInfo.InvariantCulture, out this.asDouble))
						this.lexem = JSONLexem.Number;
					else
						this.lexem = JSONLexem.Unknown;

					break;
				
				case ':':
					this.current = this.reader.Read ();
					this.lexem = JSONLexem.Colon;
					
					break;

				case 'f':
				case 'n':
				case 't':
					buffer = new StringBuilder (16);

					while (this.current >= (int)'a' && this.current <= (int)'z')
					{
						buffer.Append ((char)this.current);

						this.current = this.reader.Read ();
					}

					switch (buffer.ToString ())
					{
						case "false":
							this.lexem = JSONLexem.False;

							break;
							
						case "null":
							this.lexem = JSONLexem.Null;

							break;

						case "true":
							this.lexem = JSONLexem.True;

							break;

						default:
							this.lexem = JSONLexem.Unknown;

							break;
					}

					break;

				case '[':
					this.current = this.reader.Read ();
					this.lexem = JSONLexem.ArrayBegin;
					
					break;

				case ']':
					this.current = this.reader.Read ();
					this.lexem = JSONLexem.ArrayEnd;
					
					break;

				case '{':
					this.current = this.reader.Read ();
					this.lexem = JSONLexem.ObjectBegin;
					
					break;

				case '}':
					this.current = this.reader.Read ();
					this.lexem = JSONLexem.ObjectEnd;
					
					break;

				default:
					this.current = this.reader.Read ();
					this.lexem = JSONLexem.Unknown;

					break;
			}

			return true;
		}
		
		#endregion
	}
}
