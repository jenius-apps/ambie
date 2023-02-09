using System;

namespace AmbientSounds.Models
{
    public class RecentFocusSettings : IEquatable<RecentFocusSettings>, IComparable<RecentFocusSettings>
    {
        /// <summary>
        /// Datetime when these settings were used.
        /// </summary>
        public DateTime LastUsed { get; set; }

        public int FocusMinutes { get; set; }

        public int RestMinutes { get; set; }

        public int Repeats { get; set; }

        public int CompareTo(RecentFocusSettings? other)
        {
            if (other is null) return 1;

            return LastUsed.CompareTo(other.LastUsed);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as RecentFocusSettings);
        }

        public bool Equals(RecentFocusSettings? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.FocusMinutes == other.FocusMinutes &&
                this.RestMinutes == other.RestMinutes &&
                this.Repeats == other.Repeats;
        }

        public static bool operator ==(RecentFocusSettings? left, RecentFocusSettings? right)
        {
            // Check for null on left side.
            if (left is null)
            {
                if (right is null)
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(RecentFocusSettings? left, RecentFocusSettings? right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return FocusMinutes.GetHashCode() * RestMinutes.GetHashCode() * Repeats.GetHashCode();
        }
    }
}
