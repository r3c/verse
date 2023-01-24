using System;
using System.Collections;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Tree;

internal class Browser<TEntity> : IDisposable, IEnumerable<TEntity>
{
    private Enumerator _enumerator;

    private bool _started;

    public Browser(BrowserMove<TEntity> move)
    {
        _enumerator = new Enumerator(move);
        _started = false;
    }

    public void Dispose()
    {
        _enumerator.Dispose();
        _enumerator = null;
    }

    public bool Finish()
    {
        return _enumerator.Finish();
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        if (_started)
            throw new NotSupportedException("array cannot be enumerated more than once");

        _started = true;

        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class Enumerator : IEnumerator<TEntity>
    {
        public TEntity Current => _current;

        object IEnumerator.Current => Current;

        private int _index;

        private BrowserMove<TEntity> _move;
	
        private TEntity _current;
	
        private BrowserState _state;

        public Enumerator(BrowserMove<TEntity> move)
        {
            _index = 0;
            _move = move;
            _state = BrowserState.Continue;
        }
	
        public void Dispose()
        {
            Finish();

            _move = null;
        }

        public bool Finish()
        {
            while (_state == BrowserState.Continue)
                MoveNext();
	
            return _state == BrowserState.Success;
        }
	
        public bool MoveNext()
        {
            if (_state != BrowserState.Continue)
                return false;

            if (_move != null)
                _state = _move(_index++, out _current);
	
            return _state == BrowserState.Continue;
        }
	
        public void Reset()
        {
            throw new NotSupportedException("array cannot be enumerated more than once");
        }
    }
}