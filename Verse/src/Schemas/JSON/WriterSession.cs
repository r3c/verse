using System.IO;
using System.Text;
using Verse.EncoderDescriptors.Base;

namespace Verse.Schemas.JSON
{
	class WriterSession : IWriterSession<WriterState>
	{
		private readonly Encoding encoding;

		private readonly bool omitNull;

		public WriterSession(Encoding encoding, bool omitNull)
		{
			this.encoding = encoding;
			this.omitNull = omitNull;
		}

	    public bool Start(Stream stream, EncodeError error, out WriterState state)
	    {
	        state = new WriterState(stream, this.encoding, this.omitNull);

	        return true;
	    }

	    public void Stop(WriterState state)
		{
			state.Flush();
		}
	}
}
