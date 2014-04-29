using System;
using System.IO;

namespace Verse.Schemas.JSON
{
    public sealed class Context : IDisposable
    {
    	#region Properties

        public int  Current
        {
            get
            {
                return this.current;
            }
        }

		public int	Position
		{
			get
			{
				return this.position;
			}
		}

        #endregion

        #region Attributes

        private int             		current;

		private int						position;

        private readonly StreamReader	reader;

		#endregion

		#region Constructors

        public Context (StreamReader reader)
        {
            this.current = reader.Read ();
			this.position = 0;
            this.reader = reader;
        }

		#endregion

		#region Methods

        public void Dispose ()
        {
            this.reader.Dispose ();
        }

        public void Next ()
        {
            int c;

            c = this.reader.Read ();

			if (this.current >= 0)
				++this.position;

            this.current = c;
        }

        #endregion
    }
}
