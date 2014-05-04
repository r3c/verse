using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.BuilderDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	sealed class Writer : IWriter<WriterContext, Value>
	{
		#region Events

		public event BuildError	Error;

		#endregion

		#region Attributes / Instance

		private readonly Encoding	encoding;

		#endregion

		#region Attributes / Static

		private static readonly char[]	hexadecimal = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		#endregion

		#region Constructors

		public Writer (Encoding encoding)
		{
			this.encoding = encoding;
		}

		#endregion

		#region Methods / Public

		public bool Start (Stream stream, out WriterContext context)
		{
			context = new WriterContext (stream, this.encoding);

			return true;
		}

		public void Stop (WriterContext context)
		{
			context.Flush ();
		}

		public void WriteItems<T> (IEnumerable<T> items, IPointer<T, WriterContext, Value> pointer, WriterContext context)
		{
			IEnumerator<T>	enumerator;

			context.Push ('[');
			enumerator = items.GetEnumerator ();

			if (enumerator.MoveNext ())
			{
				pointer.Enter (enumerator.Current, this, context);

				while (enumerator.MoveNext ())
				{
					context.Push (',');

					pointer.Enter (enumerator.Current, this, context);
				}
			}

			context.Push (']');
		}

		public void WriteKey<T> (T source, string name, IPointer<T, WriterContext, Value> pointer, WriterContext context)
		{
			//context.Push ('{');

			this.WriteString (context, name);

			context.Push (':');

			pointer.Enter (source, this, context);

			//context.Push ('}');
		}

		public void WriteValue (Value value, WriterContext context)
		{
			switch (value.Type)
			{
				case Content.Boolean:
					if (value.Boolean)
					{
						context.Push ('t');
						context.Push ('r');
						context.Push ('u');
						context.Push ('e');
					}
					else
					{
						context.Push ('f');
						context.Push ('a');
						context.Push ('l');
						context.Push ('s');
						context.Push ('e');
					}

					break;

				case Content.Number:
					foreach (char c in value.Number.ToString (CultureInfo.InvariantCulture))
						context.Push (c);

					break;

				case Content.String:
					this.WriteString (context, value.String);

					break;

				default:
					context.Push ('n');
					context.Push ('u');
					context.Push ('l');
					context.Push ('l');

					break;
			}
		}

		#endregion

		#region Methods / Private

		private void OnError (int position, string message)
		{
			BuildError	error;

			error = this.Error;

			if (error != null)
				error (position, message);
		}

		private void WriteString (WriterContext context, string value)
		{
			context.Push ('"');

			foreach (char c in value)
			{
				if (c >= ' ' && c < 128)
					context.Push (c);
				else
				{
					context.Push ('\\');
					context.Push ('u');
					context.Push (Writer.hexadecimal[(c >> 12) & 0xF]);
					context.Push (Writer.hexadecimal[(c >> 8) & 0xF]);
					context.Push (Writer.hexadecimal[(c >> 4) & 0xF]);
					context.Push (Writer.hexadecimal[(c >> 0) & 0xF]); 
				}
			}

			context.Push ('"');
		}

		#endregion
	}
}
