using System;
using System.Runtime.CompilerServices;
using NativeCollections;

namespace Examples
{
    public unsafe struct StateMachine
    {
        private UnsafeDictionary<nint, nint> _states;
        private State _current;

        public StateMachine(int capacity) => _states = new UnsafeDictionary<nint, nint>(capacity);

        public void Dispose()
        {
            foreach (var state in _states.Values)
                NativeMemoryAllocator.AlignedFree((void*)state);
            _states.Dispose();
            _states = new UnsafeDictionary<nint, nint>();
        }

        public void Change<T>() where T : unmanaged, IState
        {
            var typeCode = TypeHelpers<T>.GetTypeCode();
            if (_current.IsCreated)
                _current.Exit();
            ref var state = ref _states.GetValueRefOrAddDefault(typeCode, out var exists);
            if (!exists)
                state = (nint)NativeMemoryAllocator.AlignedAllocZeroed<T>(1);
            _current = new State();
            _current.Set<T>((void*)state);
            Unsafe.AsRef<T>((void*)state).OnEnter();
        }

        public void Update()
        {
            if (_current.IsCreated)
                _current.Update();
        }
    }

    internal unsafe struct State
    {
        private void* _ptr;
        private delegate* managed<void*, void> _enter;
        private delegate* managed<void*, void> _exit;
        private delegate* managed<void*, void> _update;

        public void Set<T>(void* ptr) where T : unmanaged, IState
        {
            _ptr = ptr;
            _enter = &StateHelpers<T>.Enter;
            _exit = &StateHelpers<T>.Exit;
            _update = &StateHelpers<T>.Update;
        }

        public bool IsCreated => _ptr != null;

        public void Enter() => _enter(_ptr);

        public void Exit() => _exit(_ptr);

        public void Update() => _update(_ptr);
    }

    public interface IState
    {
        void OnEnter();

        void OnExit();

        void OnUpdate();
    }

    internal static unsafe class TypeHelpers<T>
    {
        public static nint GetTypeHandle() => typeof(T).TypeHandle.Value;

        public static nint GetTypeCode() => (nint)(delegate* managed<nint>)&GetTypeHandle;
    }

    internal static unsafe class StateHelpers<T> where T : unmanaged, IState
    {
        public static void Enter(void* state) => Unsafe.AsRef<T>(state).OnEnter();

        public static void Exit(void* state) => Unsafe.AsRef<T>(state).OnExit();

        public static void Update(void* state) => Unsafe.AsRef<T>(state).OnUpdate();
    }

    public struct TestStateA : IState
    {
        public int Value;

        public void OnEnter()
        {
            Console.WriteLine("Enter: " + GetType().Name);
        }

        public void OnExit()
        {
            Console.WriteLine("Exit: " + GetType().Name);
        }

        public void OnUpdate()
        {
            Console.WriteLine("Update: " + GetType().Name + " " + Value++);
        }
    }

    public struct TestStateB : IState
    {
        public int Value;

        public void OnEnter()
        {
            Console.WriteLine("Enter: " + GetType().Name);
        }

        public void OnExit()
        {
            Console.WriteLine("Exit: " + GetType().Name);
        }

        public void OnUpdate()
        {
            Console.WriteLine("Update: " + GetType().Name + " " + Value++);
        }
    }
}