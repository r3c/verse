using System;
using System.IO;
using Verse.EncoderDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class WriterSession : IWriterSession<WriterState>
	{
		private readonly JSONSettings settings;

		public WriterSession(JSONSettings settings)
		{
			this.settings = settings;
		}

		public bool Start(Stream stream, EncodeError error, out WriterState state)
		{
			state = new WriterState(stream, error, this.settings);

			return true;
		}

		public void Stop(WriterState state)
		{
			state.Flush();
		}
	}
}
