using System;
using System.IO;
using System.Text;
using Verse.DecoderDescriptors.Base;

namespace Verse.Schemas.JSON
{
	class ReaderSession : IReaderSession<ReaderState>
	{
		private readonly Encoding encoding;

		public ReaderSession(Encoding encoding)
		{
			this.encoding = encoding;
		}

		public bool Start(Stream stream, DecodeError error, out ReaderState state)
		{
			state = new ReaderState(stream, this.encoding, error);
			state.PullIgnored();

			if (state.Current < 0)
			{
				state.Error("empty input stream");

				return false;
			}

			return true;
		}

		public void Stop(ReaderState state)
		{
		}
	}
}
