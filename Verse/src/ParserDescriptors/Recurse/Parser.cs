using System;
using System.IO;

namespace Verse.ParserDescriptors.Recurse
{
	class Parser<T, C, V> : IParser<T>
	{
		#region Events

		public event ParserError	Error;

		#endregion

		#region Attributes

		private readonly Func<T>			constructor;

		private readonly Container<T, C, V>	container;

		private readonly IReader<C, V>		reader;

		#endregion

		#region Constructors

		public Parser (Func<T> constructor, Container<T, C, V> container, IReader<C, V> reader)
		{
			reader.Error += this.OnError;

			this.constructor = constructor;
			this.container = container;
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

				return this.reader.Read (ref output, this.container, context);
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
			ParserError	error;

			error = this.Error;

			if (error != null)
				error (position, message);
		}

		#endregion
	}
}
