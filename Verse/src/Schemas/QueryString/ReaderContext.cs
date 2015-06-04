using System.IO;
using System.Text;

namespace Verse.Schemas.QueryString
{
    internal class ReaderContext
    {
        #region Properties

        public int Current
        {
            get
            {
                return this.current;
            }
        }

        public int Position
        {
            get
            {
                return this.position;
            }
        }

        #endregion

        #region Attributes

        private int current;

        private int position;

        public bool IsField;

        private readonly StreamReader reader;

        #endregion

        #region Constructors

        public ReaderContext(Stream stream, Encoding encoding)
        {
            this.current = 0;
            this.position = 0;
            this.IsField = true;
            this.reader = new StreamReader(stream, encoding);

            this.Pull();
        }

        #endregion

        #region Methods

        public void Pull()
        {
            this.current = this.reader.Read();

            ++this.position;
        }

        #endregion
    }
}