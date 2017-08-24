namespace Memstate.Postgresql
{
    public class RingBuffer<T>
    {
        private readonly T[] _buffer;
        private int _head;
        private int _tail;
        private int _count;

        public RingBuffer(int capacity)
        {
            _buffer = new T[capacity];
            _head = capacity - 1;
        }

        public void Enqueue(T item)
        {
            _head = (_head + 1) % _buffer.Length;

            _buffer[_head] = item;

            if (_count == _buffer.Length)
            {
                _tail = (_tail + 1) % _buffer.Length;
            }
            else
            {
                _count++;
            }
        }

        public bool TryDequeue(out T item)
        {
            if (_count == 0)
            {
                item = default(T);

                return false;
            }

            item = _buffer[_tail];

            _tail = (_tail + 1) % _buffer.Length;

            _count--;

            return true;
        }

        public bool TryPeek(out T item)
        {
            if (_count == 0)
            {
                item = default(T);

                return false;
            }


            item = _buffer[_tail];

            return true;
        }
    }
}