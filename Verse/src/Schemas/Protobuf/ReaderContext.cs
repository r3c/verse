using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Verse.Schemas.Protobuf
{
    class ReaderContext
    {
        #region Properties

        public int Position
        {
            get
            {
                return this.Reader.Position;
            }
        }

        public bool HeaderToRead;

        public ProtoReader Reader;

        #endregion

        #region Constructors

        public ReaderContext(Stream stream)
        {
            this.Reader = new ProtoReader(stream, TypeModel.Create(), null);
            this.HeaderToRead = true;
        }

        public bool ReadHeader(out int index)
        {
            index = this.Reader.ReadFieldHeader();

            return index > 0;
        }

        #endregion
    }
}
