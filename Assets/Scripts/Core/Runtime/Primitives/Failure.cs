using System;

namespace PCShopEmpire3D.Core.Primitives
{
    /// <summary>
    /// A machine-readable failure. Presentation layers map Code to localized player-facing text.
    /// </summary>
    public readonly struct Failure : IEquatable<Failure>
    {
        private readonly StableId<FailureCodeScope> _code;

        private Failure(StableId<FailureCodeScope> code)
        {
            _code = code;
        }

        public static Failure None => default;

        public static Failure Uninitialized { get; } = FromCode("core.uninitialized");

        public string Code => _code.Value;

        public bool IsNone => _code.IsEmpty;

        public static Failure FromCode(string code)
        {
            return new Failure(StableId<FailureCodeScope>.Parse(code));
        }

        public bool Equals(Failure other)
        {
            return _code.Equals(other._code);
        }

        public override bool Equals(object obj)
        {
            return obj is Failure other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _code.GetHashCode();
        }

        public override string ToString()
        {
            return Code;
        }

        public static bool operator ==(Failure left, Failure right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Failure left, Failure right)
        {
            return !left.Equals(right);
        }

        private sealed class FailureCodeScope : IStableIdScope
        {
        }
    }
}
