/******************************************************************************
 * Copyright (c) 2014-2016, Pedro Ramalhete, Andreia Correia
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Concurrency Freaks nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.

 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************
 */

// ReSharper disable ALL

using System;
using System.Runtime.InteropServices;
using NativeCollections;
using static NativeCollections.PaddingHelpers;

namespace Examples
{
    /// <summary>
    ///     https://github.com/pramalhe/ConcurrencyFreaks
    /// </summary>
    public unsafe readonly struct HazardPointers : IIsCreated, IDisposable
    {
        /// <summary>
        ///     This is named 'K' in the HP paper
        /// </summary>
        public const int HP_MAX_HPS = 128;

        public const int HP_MAX_THREADS = 128;

        // private static readonly int CLPAD = CACHE_LINE_SIZE / sizeof(nint);

        /// <summary>
        ///     // This is named 'R' in the HP paper
        /// </summary>
        public const int HP_THRESHOLD_R = 0;

        // private const int MAX_RETIRED = HP_MAX_HPS * HP_MAX_THREADS; // Maximum number of retired objects per thread

        private readonly int maxHPs;
        private readonly int maxThreads;
        private readonly int hpThreshold;

        private readonly NativeArray<NativeArray<UnsafeAtomicIsize>> hp;
        private readonly NativeArray<CachePaddedList> retiredList;

        private static void defdeleter(void* t, int tid) => NativeMemoryAllocator.AlignedFree(t);
        private readonly delegate* managed<void*, int, void> deleter;

        public bool IsCreated => hp.IsCreated;

        public HazardPointers() : this(HP_MAX_HPS, HP_MAX_THREADS)
        {
        }

        public HazardPointers(int maxHPs, int maxThreads) : this(maxHPs, maxThreads, HP_THRESHOLD_R)
        {
        }

        public HazardPointers(int maxHPs, int maxThreads, int hpThreshold) : this(maxHPs, maxThreads, hpThreshold, &defdeleter)
        {
        }

        public HazardPointers(int maxHPs, int maxThreads, int hpThreshold, delegate* managed<void*, int, void> deleter)
        {
            this.maxHPs = maxHPs;
            this.maxThreads = maxThreads;
            this.hpThreshold = hpThreshold;
            this.deleter = deleter;
            this.hp = new NativeArray<NativeArray<UnsafeAtomicIsize>>(maxThreads, CACHE_LINE_SIZE);
            this.retiredList = new NativeArray<CachePaddedList>(maxThreads, CACHE_LINE_SIZE, true);
            for (int ithread = 0; ithread < maxThreads; ithread++)
            {
                hp[ithread] = new NativeArray<UnsafeAtomicIsize>(maxHPs, true);
            }
        }

        public void Dispose()
        {
            for (int ithread = 0; ithread < maxThreads; ithread++)
            {
                hp[ithread].Dispose();
                // Clear the current retired nodes
                for (var iret = 0; iret < retiredList[ithread].list.Count; iret++)
                {
                    NativeMemoryAllocator.AlignedFree((void*)retiredList[ithread].list[iret]);
                }

                retiredList[ithread].list.Dispose();
            }

            hp.Dispose();
            retiredList.Dispose();
        }

        /// <summary>
        ///     Progress Condition: wait-free bounded (by maxHPs)
        /// </summary>
        public void clear(int tid)
        {
            for (int ihp = 0; ihp < maxHPs; ihp++)
            {
                hp[tid][ihp].Store(0, Ordering.Release);
            }
        }

        /// <summary>
        ///     Progress Condition: wait-free population oblivious
        /// </summary>
        public void clearOne(int ihp, int tid)
        {
            hp[tid][ihp].Store(0, Ordering.Release);
        }

        /// <summary>
        ///     Progress Condition: lock-free
        /// </summary>
        public T* protect<T>(int index, ref UnsafeAtomicPtr<T> atom, int tid) where T : unmanaged
        {
            T* n = null;
            T* ret;
            while ((ret = atom.Load(Ordering.Acquire)) != n)
            {
                hp[tid][index].Store((nint)ret, Ordering.Release);
                n = ret;
            }

            return ret;
        }

        public void* get(int index, int tid)
        {
            return (void*)hp[tid][index].Load(Ordering.Acquire);
        }

        /// <summary>
        ///     This returns the same value that is passed as ptr, which is sometimes useful
        ///     Progress Condition: wait-free population oblivious
        /// </summary>
        public T* protectPtr<T>(int index, T* ptr, int tid) where T : unmanaged
        {
            hp[tid][index].Store((nint)ptr, Ordering.Release);
            return ptr;
        }

        /// <summary>
        ///     Progress Condition: wait-free bounded (by the number of threads squared)
        /// </summary>
        public void retire<T>(T* ptr, int tid) where T : unmanaged
        {
            retiredList[tid].list.Add((nint)ptr);
            if (retiredList[tid].list.Count < hpThreshold) return;
            for (int iret = 0; iret < retiredList[tid].list.Count;)
            {
                var obj = retiredList[tid].list[iret];
                bool canDelete = true;
                for (int tid2 = 0; tid2 < maxThreads && canDelete; tid2++)
                {
                    for (int ihp = maxHPs - 1; ihp >= 0; ihp--)
                    {
                        if (hp[tid2][ihp].Load(Ordering.Acquire) == obj)
                        {
                            canDelete = false;
                            break;
                        }
                    }
                }

                if (canDelete)
                {
                    retiredList[tid].list.RemoveAt(iret);
                    deleter((void*)obj, tid);
                    continue;
                }

                iret++;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 1 * CACHE_LINE_SIZE)]
        private struct CachePaddedList
        {
            [FieldOffset(0 * CACHE_LINE_SIZE)] public UnsafeList<nint> list;
        }
    }
}