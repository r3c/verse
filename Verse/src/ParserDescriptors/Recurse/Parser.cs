using System;
using System.IO;

namespace Verse.ParserDescriptors.Recurse
{
	class Parser<T, C, V> : IParser<T>
	{
		#region Events

		public event ParserError Error;

		#endregion

		#region Attributes

		private readonly Container<T, C, V> container;

		private readonly IReader<C, V> reader;

		#endregion

		#region Constructors

		public Parser (Container<T, C, V> container, IReader<C, V> reader)
		{
			reader.Error += this.OnError;

			this.container = container;
			this.reader = reader;
		}

		#endregion

		#region Methods / Public

		public bool Parse (Stream input, ref T output)
		{
			C context;

			if (!this.reader.Start (input, out context))
				return false;

			try
			{
				return this.reader.ReadValue (ref output, this.container, context);
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
			ParserError error;

			error = this.Error;

			if (error != null)
				error (position, message);
		}

		#endregion
	}
}
