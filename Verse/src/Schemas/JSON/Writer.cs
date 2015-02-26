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

		public event BuilderError Error
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

		private readonly Encoding encoding;

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

		public void WriteArray<T> (IEnumerable<T> items, Container<T, WriterContext, Value> container, WriterContext context)
		{
			IEnumerator<T> item;

			context.ArrayBegin ();
			item = items.GetEnumerator ();

			if (item.MoveNext ())
			{
				while (true)
				{
					this.WriteValue (item.Current, container, context);

					if (!item.MoveNext ())
						break;

					context.Next ();
				}
			}

			context.ArrayEnd ();
		}

		public void WriteValue<T> (T source, Container<T, WriterContext, Value> container, WriterContext context)
		{
			IEnumerator<KeyValuePair<string, Follow<T, WriterContext, Value>>> field;

			if (source == null)
				context.Value (new Value { Type = Content.Void });
			else if (container.items != null)
				container.items (source, this, context);
			else if (container.value != null)
				context.Value (container.value (source));
			else
			{
				context.ObjectBegin ();
				field = container.fields.GetEnumerator ();
	
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
		}

		#endregion
	}
}
