using System;
using System.IO;

namespace Verse.ParserDescriptors.Recurse
{
	class Parser<T, C, V> : IParser<T>
	{
		#region Events

		public event ParseError	Error;

		#endregion

		#region Attributes

		private readonly Func<T>			constructor;

		private readonly IPointer<T, C, V>	pointer;

		private readonly IReader<C, V>		reader;

		#endregion

		#region Constructors

		public Parser (Func<T> constructor, IPointer<T, C, V> pointer, IReader<C, V> reader)
		{
			reader.Error += this.OnError;

			this.constructor = constructor;
			this.pointer = pointer;
			this.reader = reader;
		}

		#endregion

		#region Methods / Public

		public bool Parse (Stream input, out T output)
		{
			C		context;

			if (!this.reader.Start (input, out context))
			{
				output = default (T);

				return false;
			}

			try
			{
				output = this.constructor ();

				return this.reader.Read (ref output, this.pointer, context);
			}
			finally
			{
				this.reader.Stop (context);
			}
		}

		#endregion

		#region Methods / Private

		private void OnError (int position, string message)
		{
			ParseError	error;

			error = this.Error;

			if (error != null)
				error (position, message);
		}

		#endregion
	}
}
