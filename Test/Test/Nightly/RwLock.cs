using NativeCollections;

namespace Examples
{
    public struct RwLock
    {
        private UnsafeAtomicU32 _state;

        private const uint STATE_WRITE = uint.MaxValue;
        private const uint STATE_IDLE = 0;
        private const uint MAX_READERS = STATE_WRITE - 1;

        public void Read()
        {
            var spinWait = new UnsafeSpinWait();
            var state = _state.Load(Ordering.Relaxed);
            while (true)
            {
                if (state < MAX_READERS)
                {
                    var newState = _state.CompareExchange(state + 1, state);
                    if (newState == state)
                        break;
                    state = newState;
                }
                else
                    state = _state.Load(Ordering.Relaxed);

                spinWait.SpinOnce();
            }
        }

        public void ExitRead()
        {
            var spinWait = new UnsafeSpinWait();
            var state = _state.Load(Ordering.Relaxed);
            while (true)
            {
                if (state > STATE_IDLE && state != STATE_WRITE)
                {
                    var newState = _state.CompareExchange(state - 1, state);
                    if (newState == state)
                        break;
                    state = newState;
                }
                else
                    state = _state.Load(Ordering.Relaxed);

                spinWait.SpinOnce();
            }
        }

        public void Write()
        {
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                if (_state.CompareExchange(STATE_WRITE, STATE_IDLE) == STATE_IDLE)
                    break;

                spinWait.SpinOnce();
            }
        }

        public void ExitWrite()
        {
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                if (_state.CompareExchange(STATE_IDLE, STATE_WRITE) == STATE_WRITE)
                    break;

                spinWait.SpinOnce();
            }
        }
    }
}