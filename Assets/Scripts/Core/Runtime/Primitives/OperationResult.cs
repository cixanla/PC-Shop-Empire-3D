using System;

namespace PCShopEmpire3D.Core.Primitives
{
    /// <summary>
    /// Explicit success/failure contract for operations that do not return a value.
    /// </summary>
    public readonly struct OperationResult
    {
        private const byte SuccessState = 1;
        private const byte FailureState = 2;

        private readonly byte _state;
        private readonly Failure _failure;

        private OperationResult(byte state, Failure failure)
        {
            _state = state;
            _failure = failure;
        }

        public bool IsSuccess => _state == SuccessState;

        public bool IsFailure => !IsSuccess;

        public Failure Error => _state == FailureState
            ? _failure
            : IsSuccess
                ? Failure.None
                : Failure.Uninitialized;

        public static OperationResult Success()
        {
            return new OperationResult(SuccessState, Failure.None);
        }

        public static OperationResult Fail(Failure failure)
        {
            if (failure.IsNone)
            {
                throw new ArgumentException("A failed result requires a non-empty failure code.", nameof(failure));
            }

            return new OperationResult(FailureState, failure);
        }
    }

    /// <summary>
    /// Explicit success/failure contract for operations that return a non-null value.
    /// </summary>
    public readonly struct OperationResult<T>
    {
        private const byte SuccessState = 1;
        private const byte FailureState = 2;

        private readonly byte _state;
        private readonly T _value;
        private readonly Failure _failure;

        private OperationResult(byte state, T value, Failure failure)
        {
            _state = state;
            _value = value;
            _failure = failure;
        }

        public bool IsSuccess => _state == SuccessState;

        public bool IsFailure => !IsSuccess;

        public Failure Error => _state == FailureState
            ? _failure
            : IsSuccess
                ? Failure.None
                : Failure.Uninitialized;

        public T Value
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException($"A failed result has no value. Failure code: {Error.Code}");
                }

                return _value;
            }
        }

        public static OperationResult<T> Success(T value)
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(nameof(value), "A successful result requires a non-null value.");
            }

            return new OperationResult<T>(SuccessState, value, Failure.None);
        }

        public static OperationResult<T> Fail(Failure failure)
        {
            if (failure.IsNone)
            {
                throw new ArgumentException("A failed result requires a non-empty failure code.", nameof(failure));
            }

            return new OperationResult<T>(FailureState, default, failure);
        }

        public bool TryGetValue(out T value)
        {
            value = _value;
            return IsSuccess;
        }
    }
}
