using System;
using System.Collections;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Tree;

internal class Browser<TEntity> : IDisposable, IEnumerable<TEntity>
{
    private Enumerator enumerator;

    private bool started;

    public Browser(BrowserMove<TEntity> move)
    {
        enumerator = new Enumerator(move);
        started = false;
    }

    public void Dispose()
    {
        enumerator.Dispose();
        enumerator = null;
    }

    public bool Finish()
    {
        return enumerator.Finish();
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        if (started)
            throw new NotSupportedException("array cannot be enumerated more than once");

        started = true;

        return enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class Enumerator : IEnumerator<TEntity>
    {
        public TEntity Current => current;

        object IEnumerator.Current => Current;

        private int index;

        private BrowserMove<TEntity> move;
	
        private TEntity current;
	
        private BrowserState state;

        public Enumerator(BrowserMove<TEntity> move)
        {
            index = 0;
            this.move = move;
            state = BrowserState.Continue;
        }
	
        public void Dispose()
        {
            Finish();

            move = null;
        }

        public bool Finish()
        {
            while (state == BrowserState.Continue)
                MoveNext();
	
            return state == BrowserState.Success;
        }
	
        public bool MoveNext()
        {
            if (state != BrowserState.Continue)
                return false;

            if (move != null)
                state = move(index++, out current);
	
            return state == BrowserState.Continue;
        }
	
        public void Reset()
        {
            throw new NotSupportedException("array cannot be enumerated more than once");
        }
    }
}