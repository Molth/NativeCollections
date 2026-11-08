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
    ///     Represents a strongly typed list of objects that can be accessed
    ///     by index. Provides methods to search, sort, and manipulate lists.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafeList<>))]
    public readonly unsafe struct NativeList<T> : IIsCreated, IDisposable, IEquatable<NativeList<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeList<T>* _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeList(UnsafeList<T>* handle) => _handle = handle;

        /// <summary>
        ///     Creates a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>A native list with the specified initial capacity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeList<T> Create(int capacity)
        {
            var value = UnsafeList<T>.Create(capacity);
            return new NativeList<T>(Box.New(ref value));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets a reference to the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafeList<T>>(_handle)[index];
        }

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
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
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeList<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeList<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeList<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeList<T> left, NativeList<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeList<T> left, NativeList<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Adds the given object to the end of this list. The size of the list is increased by
        ///     one. If required, the capacity of the list is doubled before adding the new element.
        /// </summary>
        /// <param name="item">The object to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item) => _handle->Add(item);

        /// <summary>
        ///     Attempts to add an object to the end of the list.
        /// </summary>
        /// <param name="item">The object to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the list;
        ///     <see langword="false" /> if the list is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(T item) => _handle->TryAdd(item);

        /// <summary>
        ///     Adds the elements of the specified collection to the end of this.
        /// </summary>
        /// <param name="buffer">The collection whose elements should be added to the end of this.</param>
        /// <exception cref="OverflowException">
        ///     Thrown when the resulting number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange([MustNotSelfAlias] ReadOnlySpan<T> buffer) => _handle->AddRange(buffer);

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
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item) => _handle->Insert(index, item);

        /// <summary>
        ///     Attempts to insert an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the list;
        ///     <see langword="false" /> if the list is already full and the item could not be added.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInsert(int index, T item) => _handle->TryInsert(index, item);

        /// <summary>
        ///     Inserts the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the resulting number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, [MustNotSelfAlias] ReadOnlySpan<T> buffer) => _handle->InsertRange(index, buffer);

        /// <summary>
        ///     Attempts to insert the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the list;
        ///     <see langword="false" /> if the list is already full and the item could not be added.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInsertRange(int index, [MustNotSelfAlias] ReadOnlySpan<T> buffer) => _handle->TryInsertRange(index, buffer);

        /// <summary>
        ///     Removes the first occurrence of a specific object from this.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully
        ///     removed; otherwise, <see langword="false" />. This method also returns
        ///     <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item) => _handle->Remove(item);

        /// <summary>
        ///     Removes the first occurrence of a specific object from this using a swap-based removal,
        ///     which maintains O(1) time complexity by moving the last element into the position of
        ///     the removed element. This operation does not preserve the original order of elements.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully
        ///     removed; otherwise, <see langword="false" />. This method also returns
        ///     <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SwapRemove(T item) => _handle->SwapRemove(item);

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => _handle->RemoveAt(index);

        /// <summary>
        ///     Removes the element at the specified index using a swap-based removal,
        ///     which is O(1) and does not preserve the order of elements. The last
        ///     element of the list is moved to the position of the removed element.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <remarks>
        ///     This method is more efficient than <see cref="RemoveAt" /> when
        ///     order is not important, as it avoids shifting subsequent elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapRemoveAt(int index) => _handle->SwapRemoveAt(index);

        /// <summary>
        ///     Removes a range of elements from this.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or <paramref name="count" /> is negative, or
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
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index) => _handle->Reverse(index);

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <param name="count">The number of elements.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index, int count) => _handle->Reverse(index, count);

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified element; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => _handle->Contains(item);

        /// <summary>
        ///     Sets the count of this to the specified value.
        /// </summary>
        /// <param name="count">The value to set the list's count to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
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
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trims the excess capacity of this based on the current count.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trims the excess capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

        /// <summary>
        ///     Determines the index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item) => _handle->IndexOf(item);

        /// <summary>
        ///     Determines the index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index) => _handle->IndexOf(item, index);

        /// <summary>
        ///     Determines the index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item, int index, int count) => _handle->IndexOf(item, index, count);

        /// <summary>
        ///     Determines the last index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(T item) => _handle->LastIndexOf(item);

        /// <summary>
        ///     Determines the last index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(T item, int index) => _handle->LastIndexOf(item, index);

        /// <summary>
        ///     Determines the last index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(T item, int index, int count) => _handle->LastIndexOf(item, index, count);

        /// <summary>
        ///     Sets the capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than the current count.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity) => _handle->SetCapacity(capacity);

        /// <summary>
        ///     Returns a span over a portion of this.
        /// </summary>
        /// <returns>A span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => _handle->AsSpan();

        /// <summary>
        ///     Returns a read-only span over a portion of this.
        /// </summary>
        /// <returns>A read-only span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => _handle->AsReadOnlySpan();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public UnsafeList<T>.Enumerator GetEnumerator() => _handle->GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}