using System;

namespace Examples
{
    public readonly struct SimpleTidGuard : IDisposable
    {
        public readonly int ThreadId;

        public SimpleTidGuard(int threadId) => ThreadId = threadId;

        public void Dispose() => SimpleTidManager.Return(ThreadId);

        public static implicit operator int(SimpleTidGuard value) => value.ThreadId;
    }
}