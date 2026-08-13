using System;
using System.Globalization;
using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Randomness;

namespace PCShopEmpire3D.Tests.EditMode.Core.Randomness
{
    public sealed class RandomStreamDerivationTests
    {
        [TestCase("0000000000000000", 0UL)]
        [TestCase("0123456789abcdef", 0x0123_4567_89AB_CDEFUL)]
        [TestCase("ffffffffffffffff", ulong.MaxValue)]
        public void RootSeedUsesStrictCanonicalRoundTrip(string text, ulong expected)
        {
            RandomRootSeed parsed = RandomRootSeed.ParseCanonical(text);

            Assert.That(parsed.Value, Is.EqualTo(expected));
            Assert.That(parsed.ToCanonicalString(), Is.EqualTo(text));
            Assert.That(parsed.ToString(), Is.EqualTo(text));
            Assert.That(RandomRootSeed.TryParseCanonical(text, out RandomRootSeed reparsed), Is.True);
            Assert.That(reparsed, Is.EqualTo(parsed));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("000000000000000")]
        [TestCase("00000000000000000")]
        [TestCase("0123456789ABCDEF")]
        [TestCase("0x123456789abcdef0")]
        [TestCase(" 123456789abcdef")]
        [TestCase("0123456789abcdeg")]
        public void RootSeedRejectsNonCanonicalInput(string text)
        {
            Assert.That(RandomRootSeed.TryParseCanonical(text, out _), Is.False);
            Assert.Throws<FormatException>(() => RandomRootSeed.ParseCanonical(text));
        }

        [Test]
        public void FirstGoldenVectorLocksFingerprintInitializationAndDraws()
        {
            RandomRootSeed rootSeed = RandomRootSeed.ParseCanonical("0000000000000000");
            StableId<RandomStreamDomainScope> domain = Domain("tests.golden.v1");
            StableId<RandomStreamContextScope> context = Context("event.0001");
            uint[] expectedDraws =
            {
                0x825F9A3FU,
                0x9E3A5650U,
                0xDED60EC6U,
                0xF277362AU,
                0x10C6D09AU,
                0xCEAA040AU
            };

            RandomStreamInitialization initialization = RandomStreamDerivation.Derive(rootSeed, domain, context);

            Assert.That(RandomStreamDerivation.Id, Is.EqualTo("sha256-framed-be-pcg32-v1"));
            Assert.That(
                RandomStreamDerivation.GetDerivationFingerprint(rootSeed, domain, context),
                Is.EqualTo("92868a93c2ce5d67d75602a2ceef8afc841523e8cf9d12732f29f600105ff722"));
            Assert.That(initialization.InitialState, Is.EqualTo(0x9286_8A93_C2CE_5D67UL));
            Assert.That(initialization.StreamSelector, Is.EqualTo(0x5756_02A2_CEEF_8AFCUL));

            DeterministicRandom random = initialization.CreateRandom();
            Assert.That(random.CaptureState().Increment, Is.EqualTo(0xAEAC_0545_9DDF_15F9UL));
            foreach (uint expected in expectedDraws)
            {
                Assert.That(random.NextUInt32(), Is.EqualTo(expected));
            }
        }

        [Test]
        public void SecondGoldenVectorLocksProductionEconomyExample()
        {
            RandomRootSeed rootSeed = RandomRootSeed.ParseCanonical("0123456789abcdef");
            StableId<RandomStreamDomainScope> domain = Domain("economy.daily-price.v1");
            StableId<RandomStreamContextScope> context = Context("market-day.00000001.product.gpu-001");
            uint[] expectedDraws =
            {
                0x34E4F82BU,
                0xF84783BFU,
                0x8C618744U,
                0xD725400EU,
                0x73317402U,
                0x3D43442CU
            };

            RandomStreamInitialization initialization = RandomStreamDerivation.Derive(rootSeed, domain, context);

            Assert.That(
                RandomStreamDerivation.GetDerivationFingerprint(rootSeed, domain, context),
                Is.EqualTo("d1367f26d5a21756de8cd1459407214be3ee09b21ca78a916b4511813cd42600"));
            Assert.That(initialization.InitialState, Is.EqualTo(0xD136_7F26_D5A2_1756UL));
            Assert.That(initialization.StreamSelector, Is.EqualTo(0x5E8C_D145_9407_214BUL));

            DeterministicRandom random = initialization.CreateRandom();
            foreach (uint expected in expectedDraws)
            {
                Assert.That(random.NextUInt32(), Is.EqualTo(expected));
            }
        }

        [Test]
        public void SameRootDomainAndContextMatchForOneThousandDraws()
        {
            RandomRootSeed seed = new RandomRootSeed(0xDEAD_BEEF_CAFE_BABEUL);
            RandomStreamInitialization initialization = RandomStreamDerivation.Derive(
                seed,
                Domain("customers.arrival.v1"),
                Context("store-day.0000042"));
            DeterministicRandom first = initialization.CreateRandom();
            DeterministicRandom second = RandomStreamDerivation.Derive(
                seed,
                Domain("customers.arrival.v1"),
                Context("store-day.0000042")).CreateRandom();

            for (int index = 0; index < 1000; index++)
            {
                Assert.That(second.NextUInt32(), Is.EqualTo(first.NextUInt32()), $"Draw {index} drifted.");
            }
        }

        [Test]
        public void ChangingAnyIdentityFieldChangesInitialization()
        {
            RandomStreamInitialization baseline = RandomStreamDerivation.Derive(
                new RandomRootSeed(10UL),
                Domain("inventory.delivery.v1"),
                Context("delivery.0001"));
            RandomStreamInitialization changedRoot = RandomStreamDerivation.Derive(
                new RandomRootSeed(11UL),
                Domain("inventory.delivery.v1"),
                Context("delivery.0001"));
            RandomStreamInitialization changedDomain = RandomStreamDerivation.Derive(
                new RandomRootSeed(10UL),
                Domain("inventory.damage.v1"),
                Context("delivery.0001"));
            RandomStreamInitialization changedContext = RandomStreamDerivation.Derive(
                new RandomRootSeed(10UL),
                Domain("inventory.delivery.v1"),
                Context("delivery.0002"));

            Assert.That(changedRoot, Is.Not.EqualTo(baseline));
            Assert.That(changedDomain, Is.Not.EqualTo(baseline));
            Assert.That(changedContext, Is.Not.EqualTo(baseline));
        }

        [Test]
        public void DerivationIsIndependentFromCallOrder()
        {
            RandomRootSeed seed = new RandomRootSeed(12345UL);
            StableId<RandomStreamDomainScope> domain = Domain("employees.performance.v1");
            StableId<RandomStreamContextScope> firstContext = Context("shift.0001.employee.001");
            StableId<RandomStreamContextScope> secondContext = Context("shift.0001.employee.002");

            RandomStreamInitialization firstBefore = RandomStreamDerivation.Derive(seed, domain, firstContext);
            RandomStreamDerivation.Derive(seed, domain, secondContext);
            RandomStreamInitialization firstAfter = RandomStreamDerivation.Derive(seed, domain, firstContext);

            Assert.That(firstAfter, Is.EqualTo(firstBefore));
        }

        [Test]
        public void LengthFramingPreventsConcatenationAmbiguity()
        {
            RandomRootSeed seed = new RandomRootSeed(42UL);
            RandomStreamInitialization first = RandomStreamDerivation.Derive(
                seed,
                Domain("a.b"),
                Context("c"));
            RandomStreamInitialization second = RandomStreamDerivation.Derive(
                seed,
                Domain("a"),
                Context("b.c"));

            Assert.That(second, Is.Not.EqualTo(first));
        }

        [Test]
        public void DerivationIsCultureIndependent()
        {
            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                RandomRootSeed seed = new RandomRootSeed(987654321UL);
                StableId<RandomStreamDomainScope> domain = Domain("economy.weekly-demand.v1");
                StableId<RandomStreamContextScope> context = Context("week.0052.product.cpu-001");
                RandomStreamInitialization invariant = RandomStreamDerivation.Derive(seed, domain, context);

                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

                Assert.That(RandomStreamDerivation.Derive(seed, domain, context), Is.EqualTo(invariant));
                Assert.That(seed.ToCanonicalString(), Is.EqualTo("000000003ade68b1"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }

        [Test]
        public void SaveMetadataRoundTripDoesNotRerollTheSameOccurrence()
        {
            RandomRootSeed savedSeed = new RandomRootSeed(0x0102_0304_0506_0708UL);
            StableId<RandomStreamDomainScope> domain = Domain("market.supplier-delay.v1");
            StableId<RandomStreamContextScope> sameOccurrence = Context("purchase-order.po-000042");
            RandomStreamInitialization beforeSave = RandomStreamDerivation.Derive(savedSeed, domain, sameOccurrence);

            RandomStreamInitialization afterLoad = RandomStreamDerivation.DeriveInitializationFromSaveMetadata(
                savedSeed.ToCanonicalString(),
                RandomStreamDerivation.Id,
                Pcg32Algorithm.Id,
                domain,
                sameOccurrence);
            RandomStreamInitialization newOccurrence = RandomStreamDerivation.DeriveInitializationFromSaveMetadata(
                savedSeed.ToCanonicalString(),
                RandomStreamDerivation.Id,
                Pcg32Algorithm.Id,
                domain,
                Context("purchase-order.po-000043"));

            Assert.That(afterLoad, Is.EqualTo(beforeSave));
            Assert.That(afterLoad.CreateRandom().NextUInt32(), Is.EqualTo(beforeSave.CreateRandom().NextUInt32()));
            Assert.That(newOccurrence, Is.Not.EqualTo(beforeSave));
        }

        [Test]
        public void SnapshotRestoresAStreamDerivedFromStableContext()
        {
            DeterministicRandom uninterrupted = RandomStreamDerivation.Derive(
                new RandomRootSeed(77UL),
                Domain("service.diagnosis.v1"),
                Context("repair-ticket.rma-0007")).CreateRandom();
            for (int index = 0; index < 73; index++)
            {
                uninterrupted.NextUInt32();
            }

            DeterministicRandom restored = DeterministicRandom.FromState(uninterrupted.CaptureState());
            for (int index = 0; index < 256; index++)
            {
                Assert.That(restored.NextUInt32(), Is.EqualTo(uninterrupted.NextUInt32()));
            }
        }

        [Test]
        public void MissingOrUnknownSaveMetadataNeverFallsBack()
        {
            StableId<RandomStreamDomainScope> domain = Domain("tests.metadata.v1");
            StableId<RandomStreamContextScope> context = Context("case.0001");

            Assert.Throws<NotSupportedException>(() => RandomStreamDerivation.DeriveInitializationFromSaveMetadata(
                "0000000000000001", null, Pcg32Algorithm.Id, domain, context));
            Assert.Throws<NotSupportedException>(() => RandomStreamDerivation.DeriveInitializationFromSaveMetadata(
                "0000000000000001", "sha256-framed-be-pcg32-v2", Pcg32Algorithm.Id, domain, context));
            Assert.Throws<NotSupportedException>(() => RandomStreamDerivation.DeriveInitializationFromSaveMetadata(
                "0000000000000001", RandomStreamDerivation.Id, null, domain, context));
            Assert.Throws<NotSupportedException>(() => RandomStreamDerivation.DeriveInitializationFromSaveMetadata(
                "0000000000000001", RandomStreamDerivation.Id, "pcg32-v2", domain, context));
            Assert.Throws<FormatException>(() => RandomStreamDerivation.DeriveInitializationFromSaveMetadata(
                null, RandomStreamDerivation.Id, Pcg32Algorithm.Id, domain, context));
        }

        [Test]
        public void EmptyStableIdentifiersAreRejected()
        {
            RandomRootSeed seed = new RandomRootSeed(1UL);

            Assert.Throws<ArgumentException>(() => RandomStreamDerivation.Derive(
                seed,
                default,
                Context("valid.context")));
            Assert.Throws<ArgumentException>(() => RandomStreamDerivation.Derive(
                seed,
                Domain("valid.domain"),
                default));
        }

        [Test]
        public void MaximumLengthStableIdentifiersRemainSupportedAndSelectorStaysInRange()
        {
            string maximumLengthId = new string('a', StableId<RandomStreamDomainScope>.MaximumLength);
            RandomStreamInitialization initialization = RandomStreamDerivation.Derive(
                new RandomRootSeed(ulong.MaxValue),
                Domain(maximumLengthId),
                Context(maximumLengthId));

            Assert.That(initialization.StreamSelector, Is.LessThanOrEqualTo(Pcg32Algorithm.MaximumStreamSelector));
        }

        private static StableId<RandomStreamDomainScope> Domain(string value)
        {
            return StableId<RandomStreamDomainScope>.Parse(value);
        }

        private static StableId<RandomStreamContextScope> Context(string value)
        {
            return StableId<RandomStreamContextScope>.Parse(value);
        }
    }
}
