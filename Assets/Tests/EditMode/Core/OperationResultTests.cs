using System;
using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Tests.EditMode.Core
{
    public sealed class OperationResultTests
    {
        [Test]
        public void SuccessHasNoFailureCode()
        {
            OperationResult result = OperationResult.Success();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error.IsNone, Is.True);
        }

        [Test]
        public void FailureCarriesMachineReadableCode()
        {
            OperationResult result = OperationResult.Fail(Failure.FromCode("inventory.insufficient-stock"));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("inventory.insufficient-stock"));
        }

        [Test]
        public void FailureCannotBeCreatedWithoutCode()
        {
            Assert.Throws<ArgumentException>(() => OperationResult.Fail(Failure.None));
        }

        [Test]
        public void GenericSuccessReturnsNonNullValue()
        {
            OperationResult<string> result = OperationResult<string>.Success("accepted");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo("accepted"));
            Assert.That(result.TryGetValue(out string value), Is.True);
            Assert.That(value, Is.EqualTo("accepted"));
        }

        [Test]
        public void GenericSuccessRejectsNullReference()
        {
            Assert.Throws<ArgumentNullException>(() => OperationResult<string>.Success(null));
        }

        [Test]
        public void GenericFailureDoesNotExposeAValue()
        {
            OperationResult<string> result = OperationResult<string>.Fail(Failure.FromCode("order.rejected"));

            Assert.That(result.TryGetValue(out string value), Is.False);
            Assert.That(value, Is.Null);
            Assert.Throws<InvalidOperationException>(() => _ = result.Value);
        }

        [Test]
        public void DefaultResultFailsSafelyAsUninitialized()
        {
            OperationResult result = default;
            OperationResult<int> genericResult = default;

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(Failure.Uninitialized));
            Assert.That(genericResult.IsFailure, Is.True);
            Assert.That(genericResult.Error, Is.EqualTo(Failure.Uninitialized));
        }
    }
}
