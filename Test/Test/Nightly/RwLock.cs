using System.Runtime.CompilerServices;
using NativeCollections;

namespace Examples
{
    public struct RwLock
    {
        private UnsafeAtomicU32 _state;

        private const uint WRITER_MASK = unchecked((uint)(1 << 31));
        private const uint MAX_READERS = WRITER_MASK - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead()
        {
            var state = _state.Load(Ordering.Acquire);
            return state < MAX_READERS && _state.CompareExchange(state + 1, state) == state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read()
        {
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                var state = _state.Load(Ordering.Acquire);
                if (state < MAX_READERS && _state.CompareExchange(state + 1, state) == state)
                    break;

                spinWait.SpinOnce(-1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitRead() => _state.Sub(1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWrite()
        {
            var state = _state.Load(Ordering.Acquire);
            if ((state & WRITER_MASK) == 0 && _state.CompareExchange(state | WRITER_MASK, state) == state)
            {
                var spinWait = new UnsafeSpinWait();
                while ((_state.Load(Ordering.Acquire) & ~WRITER_MASK) != 0)
                    spinWait.SpinOnce(-1);

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write()
        {
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                var state = _state.Load(Ordering.Acquire);
                if ((state & WRITER_MASK) == 0 && _state.CompareExchange(state | WRITER_MASK, state) == state)
                    break;

                spinWait.SpinOnce(-1);
            }

            spinWait.Reset();
            while ((_state.Load(Ordering.Acquire) & ~WRITER_MASK) != 0)
                spinWait.SpinOnce(-1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite() => _state.Store(0, Ordering.Release);
    }
}