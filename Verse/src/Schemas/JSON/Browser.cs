using System;
using System.Collections;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Browser<T> : IBrowser<T>
	{
		#region Properties / Public

		public T Current
		{
			get
			{
				return this.current;
			}
		}

		public bool Success
		{
			get
			{
				return this.state == BrowserState.Success;
			}
		}

		#endregion

		#region Properties / Explicit

		object IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		#endregion

		#region Attributes

		private T current;

		private int index;

		private BrowserMove<T> move;

		private BrowserState state;

		#endregion

		#region Constructors

		public Browser (BrowserMove<T> move)
		{
			this.index = 0;
			this.move = move;
			this.state = BrowserState.Continue;
		}

		#endregion

		#region Methods

		public void Dispose ()
		{
			this.move = null;
		}

		public bool MoveNext ()
		{
			if (this.state != BrowserState.Continue)
				return false;

			this.state = this.move (this.index++, out this.current);

			return this.state == BrowserState.Continue;
		}

		public void Reset ()
		{
			throw new NotSupportedException ("can't iterate over array twice");
		}

		#endregion
	}
}
