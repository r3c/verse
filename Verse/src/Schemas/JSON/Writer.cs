using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Verse.BuilderDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Writer : IWriter<WriterContext, Value>
	{
		#region Events

		public event BuildError	Error
		{
			add
			{
			}
			remove
			{
			}
		}

		#endregion

		#region Attributes

		private readonly Encoding	encoding;

		#endregion

		#region Constructors

		public Writer (Encoding encoding)
		{
			this.encoding = encoding;
		}

		#endregion

		#region Methods

		public bool Start (Stream stream, out WriterContext context)
		{
			context = new WriterContext (stream, this.encoding);

			return true;
		}

		public void Stop (WriterContext context)
		{
			context.Flush ();
		}

		public void Write<T> (T source, Container<T, WriterContext, Value> container, WriterContext context)
		{
			if (container.value != null)
				container.value (source, this, context);
			else if (container.items != null)
				container.items (source, this, context);
			else
				this.WriteFields (source, container.fields, context);
		}

		public void WriteFields<T> (T source, IEnumerable<KeyValuePair<string, Follow<T, WriterContext, Value>>> fields, WriterContext context)
		{
			IEnumerator<KeyValuePair<string, Follow<T, WriterContext, Value>>>	field;

			context.ObjectBegin ();
			field = fields.GetEnumerator ();

			if (field.MoveNext ())
			{
				while (true)
				{
					context.Key (field.Current.Key);

					field.Current.Value (source, this, context);

					if (!field.MoveNext ())
						break;

					context.Next ();
				}
			}

			context.ObjectEnd ();
		}

		public void WriteItems<T> (IEnumerable<T> items, Container<T, WriterContext, Value> container, WriterContext context)
		{
			IEnumerator<T>	item;

			context.ArrayBegin ();
			item = items.GetEnumerator ();

			if (item.MoveNext ())
			{
				while (true)
				{
					this.Write (item.Current, container, context);

					if (!item.MoveNext ())
						break;

					context.Next ();
				}
			}

			context.ArrayEnd ();
		}

		public void WriteValue (Value value, WriterContext context)
		{
			switch (value.Type)
			{
				case Content.Boolean:
					context.Boolean (value.Boolean);

					break;

				case Content.Number:
					context.Number (value.Number);

					break;

				case Content.String:
					context.String (value.String);

					break;

				default:
					context.Null ();

					break;
			}
		}

		#endregion
	}
}
