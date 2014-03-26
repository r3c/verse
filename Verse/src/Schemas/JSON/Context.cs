using System;
using System.IO;

namespace Verse.Schemas.JSON
{
    public class Context : IDisposable
    {
    	#region Properties

        public int  Current
        {
            get
            {
                return current;
            }
        }

        #endregion

        #region Attributes

        private int             		current;

        private readonly StreamReader	reader;

		#endregion

		#region Constructors

        public Context (StreamReader reader)
        {
            this.current = reader.Read ();
            this.reader = reader;
        }

		#endregion

		#region Methods

        public void Dispose ()
        {
            this.reader.Dispose ();
        }

        public bool Next ()
        {
            int c;

            c = this.reader.Read ();

            this.current = c;

            return c >= 0;
        }

        public bool Skip (char character)
        {
            return this.current == (int)character && this.Next ();
        }

        #endregion
    }
}
