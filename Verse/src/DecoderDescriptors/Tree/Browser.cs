using System;
using System.Collections;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Tree
{
	class Browser<TEntity> : IDisposable, IEnumerable<TEntity>
	{
		private Enumerator enumerator;

		private bool started;

		public Browser(BrowserMove<TEntity> move)
		{
			this.enumerator = new Enumerator(move);
			this.started = false;
		}

		public void Dispose()
		{
			this.enumerator.Dispose();
			this.enumerator = null;
		}

		public bool Finish()
		{
			return this.enumerator.Finish();
		}

		public IEnumerator<TEntity> GetEnumerator()
		{
			if (this.started)
				throw new NotSupportedException("array cannot be enumerated more than once");

			this.started = true;

			return this.enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private class Enumerator : IEnumerator<TEntity>
		{
			public TEntity Current => this.current;

		    object IEnumerator.Current => this.Current;

		    private int index;

			private BrowserMove<TEntity> move;
	
			private TEntity current;
	
			private BrowserState state;

			public Enumerator(BrowserMove<TEntity> move)
			{
				this.index = 0;
				this.move = move;
				this.state = BrowserState.Continue;
			}
	
			public void Dispose()
			{
				this.move = null;
			}

			public bool Finish()
			{
				while (this.state == BrowserState.Continue)
					this.MoveNext();
	
				return this.state == BrowserState.Success;
			}
	
			public bool MoveNext()
			{
				if (this.state != BrowserState.Continue)
					return false;
	
				this.state = this.move(this.index++, out this.current);
	
				return this.state == BrowserState.Continue;
			}
	
			public void Reset()
			{
				throw new NotSupportedException("array cannot be enumerated more than once");
			}
		}
	}
}