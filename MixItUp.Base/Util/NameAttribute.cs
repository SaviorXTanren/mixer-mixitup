using System;

namespace MixItUp.Base.Util
{
    /// <summary>
    /// An attribute that tracks the public-facing name of an item.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NameAttribute : Attribute
    {
        /// <summary>
        /// The default, public-facing name.
        /// </summary>
        public static readonly NameAttribute Default;

        /// <summary>
        /// The public-facing name of the item.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Creates a new instance of the NameAttribute class.
        /// </summary>
        public NameAttribute() : this(string.Empty) { }

        /// <summary>
        /// Creates a new instance of the NameAttribute class with a specified name.
        /// </summary>
        /// <param name="name">The public-facing name</param>
        public NameAttribute(string name) { this.Name = name; }

        /// <summary>
        /// Checks whether the following objects are equal.
        /// </summary>
        /// <param name="obj">The other object to check</param>
        /// <returns>Whether the two objects are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is NameAttribute)
            {
                NameAttribute other = (NameAttribute)obj;
                return this.Name.Equals(other.Name);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this string.
        /// </summary>
        /// <returns>Returns the hash code for this string.</returns>
        public override int GetHashCode() { return this.Name.GetHashCode(); }

        /// <summary>
        /// Whether this instance is the default attribute.
        /// </summary>
        /// <returns>true if this instance is the default attribute for the class; otherwise, false.</returns>
        public override bool IsDefaultAttribute() { return this.Equals(NameAttribute.Default); }
    }
}
