using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Tests.EditMode.Core
{
    public sealed class StableIdTests
    {
        [Test]
        public void ParsePreservesValidCanonicalValue()
        {
            const string value = "product.gpu-5500_a1";

            StableId<ProductScope> id = StableId<ProductScope>.Parse(value);

            Assert.That(id.Value, Is.EqualTo(value));
            Assert.That(id.IsEmpty, Is.False);
            Assert.That(id.ToString(), Is.EqualTo(value));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Product.gpu")]
        [TestCase("product gpu")]
        [TestCase(".product")]
        [TestCase("product-")]
        [TestCase("ürün")]
        public void TryParseRejectsNonCanonicalValue(string value)
        {
            bool parsed = StableId<ProductScope>.TryParse(value, out StableId<ProductScope> id);

            Assert.That(parsed, Is.False);
            Assert.That(id.IsEmpty, Is.True);
        }

        [Test]
        public void TryParseRejectsValueLongerThanMaximum()
        {
            string value = new string('a', StableId<ProductScope>.MaximumLength + 1);

            Assert.That(StableId<ProductScope>.TryParse(value, out _), Is.False);
        }

        [Test]
        public void EqualityIsOrdinalWithinTheSameScope()
        {
            StableId<ProductScope> first = StableId<ProductScope>.Parse("product.gpu-1");
            StableId<ProductScope> same = StableId<ProductScope>.Parse("product.gpu-1");
            StableId<ProductScope> different = StableId<ProductScope>.Parse("product.gpu-2");

            Assert.That(first, Is.EqualTo(same));
            Assert.That(first.GetHashCode(), Is.EqualTo(same.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(different));
        }

        [Test]
        public void ScopeCreatesDistinctCompileTimeIdentityTypes()
        {
            object product = StableId<ProductScope>.Parse("shared-1");
            object inventory = StableId<InventoryScope>.Parse("shared-1");

            Assert.That(product.GetType(), Is.Not.EqualTo(inventory.GetType()));
        }

        private sealed class ProductScope : IStableIdScope
        {
        }

        private sealed class InventoryScope : IStableIdScope
        {
        }
    }
}
