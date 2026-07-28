using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native list
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafeList<>))]
    public readonly unsafe struct NativeList<T> : IIsCreated, IDisposable, IEquatable<NativeList<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeList<T>* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeList(int capacity)
        {
            var value = new UnsafeList<T>(capacity);
            _handle = Box.New(ref value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafeList<T>>(_handle)[index];
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafeList<T>>(_handle)[index];
        }

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->Capacity = value;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeList<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeList<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeList<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeList<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeList<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeList<T> left, NativeList<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeList<T> left, NativeList<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Adds the given object to the end of this list. The size of the list is
        ///     increased by one. If required, the capacity of the list is doubled
        ///     before adding the new element.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item) => _handle->Add(item);

        /// <summary>
        ///     Attempts to add an object to the end of the list.
        /// </summary>
        /// <param name="item">The object to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the list;
        ///     <see langword="false" /> if the list is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in T item) => _handle->TryAdd(item);

        /// <summary>
        ///     Adds the elements of the specified collection to the end of this.
        /// </summary>
        /// <param name="buffer">The collection whose elements should be added to the end of this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> buffer) => _handle->AddRange(buffer);

        /// <summary>
        ///     Attempts to add the elements of the specified collection to the end of this.
        /// </summary>
        /// <param name="buffer">The collection whose elements should be added to the end of this.</param>
        /// <returns>
        ///     <see langword="true" /> if the items were successfully added to the list;
        ///     <see langword="false" /> if the list is already full and the items could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAddRange(ReadOnlySpan<T> buffer) => _handle->TryAddRange(buffer);

        /// <summary>
        ///     Inserts an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item) => _handle->Insert(index, item);

        /// <summary>
        ///     Attempts to insert an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the list;
        ///     <see langword="false" /> if the list is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInsert(int index, in T item) => _handle->TryInsert(index, item);

        /// <summary>
        ///     Inserts the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, ReadOnlySpan<T> buffer) => _handle->InsertRange(index, buffer);

        /// <summary>
        ///     Attempts to insert the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the list;
        ///     <see langword="false" /> if the list is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInsertRange(int index, ReadOnlySpan<T> buffer) => _handle->TryInsertRange(index, buffer);

        /// <summary>
        ///     Removes the first occurrence of a specific object from this.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully removed;
        ///     otherwise, <see langword="false" />.
        ///     This method also returns <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item) => _handle->Remove(item);

        /// <summary>
        ///     Removes the first occurrence of a specific object from this using a swap-based removal,
        ///     which maintains O(1) time complexity by moving the last element into the position of the removed element.
        ///     This operation does not preserve the original order of elements.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully removed;
        ///     otherwise, <see langword="false" />. This method also returns <see langword="false" /> if
        ///     <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SwapRemove(in T item) => _handle->SwapRemove(item);

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => _handle->RemoveAt(index);

        /// <summary>
        ///     Removes the element at the specified index using a swap-based removal,
        ///     which is O(1) and does not preserve the order of elements.
        ///     The last element of the list is moved to the position of the removed element.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <remarks>
        ///     This method is more efficient than <see cref="RemoveAt" /> when order is not important,
        ///     as it avoids shifting subsequent elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapRemoveAt(int index) => _handle->SwapRemoveAt(index);

        /// <summary>
        ///     Removes a range of elements from this.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="count" /> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count) => _handle->RemoveRange(index, count);

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse() => _handle->Reverse();

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index) => _handle->Reverse(index);

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <param name="count">The number of elements.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index, int count) => _handle->Reverse(index, count);

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>true if this contains the specified element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item) => _handle->Contains(item);

        /// <summary>
        ///     Sets the count of this to the specified value.
        /// </summary>
        /// <param name="count">The value to set the list's count to.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="count" /> is negative.
        /// </exception>
        /// <remarks>
        ///     When increasing the count, uninitialized data is being exposed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCount(int count) => _handle->SetCount(count);

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item) => _handle->IndexOf(item);

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index) => _handle->IndexOf(item, index);

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index, int count) => _handle->IndexOf(item, index, count);

        /// <summary>
        ///     Determines the last index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item) => _handle->LastIndexOf(item);

        /// <summary>
        ///     Determines the last index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item, int index) => _handle->LastIndexOf(item, index);

        /// <summary>
        ///     Determines the last index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item, int index, int count) => _handle->LastIndexOf(item, index, count);

        /// <summary>
        ///     Sets the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity) => _handle->SetCapacity(capacity);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => _handle->AsSpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => _handle->AsSpan(start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => _handle->AsSpan(start, length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => _handle->AsReadOnlySpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => _handle->AsReadOnlySpan(start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => _handle->AsReadOnlySpan(start, length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeList<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public UnsafeList<T>.Enumerator GetEnumerator() => _handle->GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}