using System;
using System.Collections.Generic;

namespace Core
{
    public sealed class Timestamp : IEquatable<Timestamp>, IComparable<Timestamp>
    {
        public static readonly Timestamp MinValue = new Timestamp(DateTime.MinValue.Ticks);
        public static readonly Timestamp MaxValue = new Timestamp(DateTime.MaxValue.Ticks);

        public Timestamp(DateTime timestamp)
            : this(timestamp.ToUniversalTime().Ticks)
        {
        }

        private Timestamp()
            : this(MinValue.Ticks)
        {
        }

        public Timestamp(DateTimeOffset timestamp)
            : this(timestamp.UtcTicks)
        {
        }

        public Timestamp(long ticks)
        {
            if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                throw new ArgumentOutOfRangeException(nameof(ticks), $"Ticks {ticks} is not in range [{DateTime.MinValue.Ticks}, {DateTime.MaxValue.Ticks}]");
            Ticks = ticks;
        }

        public static Timestamp Now => new Timestamp(PreciseTimestampGenerator.Instance.NowTicks());

        public long Ticks { get; }

        public DateTime ToDateTime()
        {
            return new DateTime(Ticks, DateTimeKind.Utc);
        }

        public DateTimeOffset ToDateTimeOffset()
        {
            return new DateTimeOffset(Ticks, TimeSpan.Zero);
        }

        public override string ToString()
        {
            return $"Ticks: {Ticks}, DateTime: {ToDateTime():O}";
        }

        public bool Equals(Timestamp? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Ticks == other.Ticks;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((Timestamp)obj);
        }

        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }

        public static bool operator ==(Timestamp? left, Timestamp? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Timestamp? left, Timestamp? right)
        {
            return !Equals(left, right);
        }

        public int CompareTo(Timestamp? other)
        {
            return other == null ? 1 : Comparer<long>.Default.Compare(Ticks, other.Ticks);
        }

        public static bool operator <(Timestamp? left, Timestamp? right)
        {
            return Comparer<Timestamp>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(Timestamp? left, Timestamp? right)
        {
            return Comparer<Timestamp>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(Timestamp? left, Timestamp? right)
        {
            return Comparer<Timestamp>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(Timestamp? left, Timestamp? right)
        {
            return Comparer<Timestamp>.Default.Compare(left, right) >= 0;
        }

        public static Timestamp operator +(Timestamp left, TimeSpan right)
        {
            var ticks = left.Ticks + right.Ticks;
            return new Timestamp(ticks);
        }

        public static Timestamp operator -(Timestamp left, TimeSpan right)
        {
            var ticks = left.Ticks - right.Ticks;
            return new Timestamp(ticks);
        }

        public static TimeSpan operator -(Timestamp left, Timestamp right)
        {
            var ticks = left.Ticks - right.Ticks;
            return new TimeSpan(ticks);
        }

        public Timestamp Subtract(TimeSpan value)
        {
            return this - value;
        }

        public TimeSpan Subtract(Timestamp value)
        {
            return this - value;
        }

        public Timestamp Add(TimeSpan value)
        {
            return this + value;
        }

        public Timestamp AddDays(double value)
        {
            return this + TimeSpan.FromDays(value);
        }

        public Timestamp AddHours(double value)
        {
            return this + TimeSpan.FromHours(value);
        }

        public Timestamp AddMinutes(double value)
        {
            return this + TimeSpan.FromMinutes(value);
        }

        public Timestamp AddSeconds(double value)
        {
            return this + TimeSpan.FromSeconds(value);
        }

        public Timestamp AddMilliseconds(double value)
        {
            return this + TimeSpan.FromMilliseconds(value);
        }

        public Timestamp AddMicroseconds(int value)
        {
            return AddTicks(PreciseTimestampGenerator.TicksPerMicrosecond * value);
        }

        public Timestamp AddTicks(long value)
        {
            return this + TimeSpan.FromTicks(value);
        }

        public Timestamp Floor(TimeSpan precision)
        {
            if (precision.Ticks <= 0)
                throw new InvalidOperationException($"Invalid precision: {precision}");
            return new Timestamp(Ticks / precision.Ticks * precision.Ticks);
        }
    }
}