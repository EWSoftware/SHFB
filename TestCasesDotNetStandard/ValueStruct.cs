using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ipipe.Core
{

    /// <summary>
    /// Result class that encapsulates data and returns the data depending on the 
    /// return type either as single value or array data. It also supports fallback mechanisms to return default values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Value<T> : IEnumerable<T>, IEquatable<Value<T>>
    {
        /// <summary>
        /// Returns an empty Value for the given generic type.
        /// </summary>
        public static readonly Value<T> Empty = default;

        private readonly bool IsInstance;

        /// <summary>
        /// Gets if the Value contains no elements.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => !IsInstance || (Data == null) || (Data?.Length == 0);
        }

        /// <summary>
        /// Indexed access to the internal data.
        /// </summary>
        /// <param name="idx_in"></param>
        /// <returns></returns>
        public ref readonly T this[int idx_in]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => ref Data[idx_in];
        }

        /// <summary>
        /// Length
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => Data?.Length ?? 0;
        }

        /// <summary>
        /// Holds the data.
        /// </summary>
        public readonly T[] Data;

        /// <summary>
        /// Creates a new instance of ResultValue.
        /// </summary>
        /// <param name="data_in"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Value(T data_in)
        {
            IsInstance = true;
            Data = new T[] { data_in };
        }

        /// <summary>
        /// Creates a new instance of ResultValue.
        /// </summary>
        /// <param name="data_in"></param>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Value(params T[] data_in)
        {
            IsInstance = true;
            Data = data_in ?? Array.Empty<T>();
        }

        /// <summary>
        /// Implicit operator for scalar values.
        /// </summary>
        /// <param name="elementValue_in"></param>
        /// <returns></returns>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static implicit operator T(Value<T> elementValue_in)
        {
            if(elementValue_in.Data == null)
                throw new NullReferenceException("No value available.");

            if(elementValue_in.Data.Length == 0)
                return default;

            return elementValue_in.Data[0];
        }

        /// <summary>
        /// Implicit operator for array values.
        /// </summary>
        /// <param name="elementValue_in"></param>
        /// <returns></returns>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static implicit operator T[](Value<T> elementValue_in)
        {
            if(elementValue_in.Data == null)
                return Array.Empty<T>();

            return elementValue_in.Data;
        }

        /// <summary>
        /// Implicit operator for array values.
        /// </summary>
        /// <param name="elementValue_in"></param>
        /// <returns></returns>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static implicit operator ReadOnlySpan<T>(Value<T> elementValue_in)
        {
            if(elementValue_in.Data == null)
                return Array.Empty<T>();

            return elementValue_in.Data;
        }

        /// <summary>
        /// Implicit operator for array values.
        /// </summary>
        /// <param name="elementValue_in"></param>
        /// <returns></returns>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static implicit operator Span<T>(Value<T> elementValue_in)
        {
            if(elementValue_in.Data == null)
                return Array.Empty<T>();

            return elementValue_in.Data;
        }

        #region IEnumerable<T>

        /// <summary>
        /// Returns the typed enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Data).GetEnumerator();

        /// <summary>
        /// Returns the enumerator.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Data.GetEnumerator();

        #endregion

        #region Object

        /// <inheritdoc />
        public override int GetHashCode() => ToString().GetHashCode();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override bool Equals(object obj)
        {
            return obj switch
            {
                T value1 => Equals(new Value<T>(value1)),
                T[] value2 => Equals(new Value<T>(value2)),
                Value<T> value3 => Equals(value3),
                _ => false,
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if(!IsInstance)
                return string.Empty;

            T aDefaultValue = default;

            if(Data.Length > 0)
                return Data[0].ToString();

            return aDefaultValue != null ? aDefaultValue.ToString() : string.Empty;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Equals(Value<T> other)
        {
            if(!IsInstance && !other.IsInstance)
                return true;

            if(Data == null || other.Data == null)
                return other.Data == Data;

            return Data.SequenceEqual(other.Data);
        }

        /// <summary>
        /// Equal
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>A Boolean</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator ==(Value<T> left, Value<T> right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>A Boolean</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator !=(Value<T> left, Value<T> right) => !(left == right);

        #endregion
    }
}