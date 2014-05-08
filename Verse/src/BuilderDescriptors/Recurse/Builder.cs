using System;
using System.IO;

namespace Verse.BuilderDescriptors.Recurse
{
	class Builder<T, C, V> : IBuilder<T>
	{
		#region Events

		public event BuildError	Error;

		#endregion

		#region Attributes

		private readonly Container<T, C, V>	container;

		private readonly IWriter<C, V>		writer;

		#endregion

		#region Constructors

		public Builder (Container<T, C, V> container, IWriter<C, V> writer)
		{
			writer.Error += this.OnError;

			this.container = container;
			this.writer = writer;
		}

		#endregion

		#region Methods / Public

		public bool Build (T input, Stream output)
		{
			C	context;

			if (!this.writer.Start (output, out context))
				return false;

			try
			{
				this.writer.Write (input, this.container, context);
			}
			finally
			{
				this.writer.Stop (context);
			}

			return true;
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

		#endregion
	}
}
