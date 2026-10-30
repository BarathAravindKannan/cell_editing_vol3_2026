using System;
using System.Collections.Generic;
using System.Linq;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Represents a composite (multi-field) primary key for DataGrid rows with multiple primary key columns.
    /// Implements proper equality and hash code computation for use as dictionary keys.
    /// </summary>
    internal sealed class CompositeKey : IEquatable<CompositeKey>
    {
        private readonly object?[] _values;

        /// <summary>
        /// Initializes a new instance of the CompositeKey class with the specified key values.
        /// </summary>
        /// <param name="values">The primary key field values that make up this composite key.</param>
        /// <exception cref="ArgumentNullException">Thrown when values is null.</exception>
        internal CompositeKey(params object?[] values)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>
        /// Determines whether two CompositeKey instances are equal by comparing all key values in sequence.
        /// </summary>
        /// <param name="other">The CompositeKey to compare with this instance.</param>
        /// <returns>true if both CompositeKey instances have equal values in the same order; otherwise, false.</returns>
        public bool Equals(CompositeKey? other)
        {
            if (other == null)
            {
                return false;
            }

            if (_values.Length != other._values.Length)
            {
                return false;
            }

            // Compare each value using SequenceEqual for null-safe comparison
            for (int i = 0; i < _values.Length; i++)
            {
                if (!Equals(_values[i], other._values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this CompositeKey instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>true if obj is a CompositeKey equal to this instance; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as CompositeKey);
        }

        /// <summary>
        /// Computes a hash code for this CompositeKey based on all key values.
        /// Uses HashCode to combine individual value hashes, ensuring different key combinations
        /// produce different hash codes (as required for proper dictionary keying).
        /// </summary>
        /// <returns>A hash code for this CompositeKey instance.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var value in _values)
            {
                hash.Add(value);
            }
            return hash.ToHashCode();
        }
    }
}
