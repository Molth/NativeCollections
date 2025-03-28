﻿using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Binary search helpers
    /// </summary>
    internal static unsafe class BinarySearchHelpers
    {
        /// <summary>
        ///     Find
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <param name="comparable">Comparable</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Find<TKey>(TKey* start, int length, in TKey comparable) where TKey : unmanaged, IComparable<TKey>
        {
            var low = 0;
            var high = length - 1;
            while (low <= high)
            {
                var i = (int)(((uint)high + (uint)low) >> 1);
                var c = comparable.CompareTo(*(start + i));
                switch (c)
                {
                    case 0:
                        return i;
                    case > 0:
                        low = i + 1;
                        break;
                    default:
                        high = i - 1;
                        break;
                }
            }

            return ~low;
        }
    }
}