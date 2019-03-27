using System.IO;
using System.Text;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.Schemas.QueryString
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

			if (state.Current < 0)
			{
				state.Error("empty input stream");

				return false;
			}

			if (state.Current != '?')
			{
				state.Error("query string must start with a '?' character");

				return false;
			}

			state.Pull();

			return true;
		}

		public void Stop(ReaderState context)
		{
		}
	}
}
