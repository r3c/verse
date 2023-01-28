using System;
using System.Collections;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Tree;

internal class ArrayIterator<TElement> : IDisposable, IEnumerable<TElement>
{
    private readonly Enumerator _enumerator;

    private bool _started;

    public ArrayIterator(ArrayReader<TElement> move)
    {
        _enumerator = new Enumerator(move);
        _started = false;
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    public bool Flush()
    {
        return _enumerator.Flush();
    }

    public IEnumerator<TElement> GetEnumerator()
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

    private class Enumerator : IEnumerator<TElement>
    {
        public TElement Current => _current;

        object? IEnumerator.Current => _current;
	
        private TElement _current;
	
        private int _index;

        private readonly ArrayReader<TElement> _reader;

        private ArrayState _state;

        public Enumerator(ArrayReader<TElement> reader)
        {
            _current = default!;
            _index = 0;
            _reader = reader;
            _state = ArrayState.NextElement;
        }
	
        public void Dispose()
        {
            Flush();
        }

        public bool Flush()
        {
            while (_state == ArrayState.NextElement)
                MoveNext();
	
            return _state == ArrayState.EndOfArray;
        }
	
        public bool MoveNext()
        {
            if (_state != ArrayState.NextElement)
                return false;

            var result = _reader(_index++);

            _current = result.Current!;
            _state = result.State;
	
            return _state == ArrayState.NextElement;
        }
	
        public void Reset()
        {
            throw new NotSupportedException("array enumeration cannot be reset");
        }
    }
}