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

		public void Write<T> (T source, Pointer<T, WriterContext, Value> pointer, WriterContext context)
		{
			int	index;

			index = 0;

			if (pointer.value != null)
				this.WriteValue (pointer.value (source), context);
			else if (pointer.items != null)
			{
				context.Push ('[');

				pointer.items (source, this, context);

				context.Push (']');
			}
			else
			{
				context.Push ('{');

				foreach (KeyValuePair<string, Follow<T, WriterContext, Value>> field in pointer.fields)
				{
					if (index++ > 0)
						context.Push (',');

					this.WriteString (context, field.Key);

					context.Push (':');

					field.Value (source, this, context);
				}
	
				context.Push ('}');
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

		private void WriteValue (Value value, WriterContext context)
		{
			switch (value.Type)
			{
				case Content.Boolean:
					context.Push (value.Boolean ? "true" : "false");

					break;

				case Content.Number:
					context.Push (value.Number.ToString (CultureInfo.InvariantCulture));

					break;

				case Content.String:
					this.WriteString (context, value.String);

					break;

				default:
					context.Push ("null");

					break;
			}
		}

		#endregion
	}
}
