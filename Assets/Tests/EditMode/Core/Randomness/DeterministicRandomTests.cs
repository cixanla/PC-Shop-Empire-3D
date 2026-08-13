using System;
using NUnit.Framework;
using PCShopEmpire3D.Core.Randomness;

namespace PCShopEmpire3D.Tests.EditMode.Core.Randomness
{
    public sealed class DeterministicRandomTests
    {
        [Test]
        public void CreateMatchesOfficialGoldenVector()
        {
            uint[] expected =
            {
                0xA15C02B7U,
                0x7B47F409U,
                0xBA1D3330U,
                0x83D2F293U,
                0xBFA4784BU,
                0xCBED606EU
            };

            DeterministicRandom random = DeterministicRandom.Create(42UL, 54UL);

            foreach (uint value in expected)
            {
                Assert.That(random.NextUInt32(), Is.EqualTo(value));
            }

            Pcg32State finalState = random.CaptureState();
            Assert.That(finalState.State, Is.EqualTo(0xBEB6_D0B7_3FDB_974AUL));
            Assert.That(finalState.Increment, Is.EqualTo(0x0000_0000_0000_006DUL));
        }

        [Test]
        public void SameSeedAndStreamMatchForOneThousandDraws()
        {
            DeterministicRandom first = DeterministicRandom.Create(0x0123_4567_89AB_CDEFUL, 9001UL);
            DeterministicRandom second = DeterministicRandom.Create(0x0123_4567_89AB_CDEFUL, 9001UL);

            for (int index = 0; index < 1000; index++)
            {
                Assert.That(second.NextUInt32(), Is.EqualTo(first.NextUInt32()), $"Draw {index} drifted.");
            }
        }

        [Test]
        public void DifferentStreamsAreDistinctAndRepeatable()
        {
            DeterministicRandom first = DeterministicRandom.Create(123UL, 11UL);
            DeterministicRandom firstReplay = DeterministicRandom.Create(123UL, 11UL);
            DeterministicRandom second = DeterministicRandom.Create(123UL, 12UL);
            DeterministicRandom secondReplay = DeterministicRandom.Create(123UL, 12UL);
            bool foundDifference = false;

            for (int index = 0; index < 128; index++)
            {
                uint firstValue = first.NextUInt32();
                uint secondValue = second.NextUInt32();
                Assert.That(firstReplay.NextUInt32(), Is.EqualTo(firstValue));
                Assert.That(secondReplay.NextUInt32(), Is.EqualTo(secondValue));
                foundDifference |= firstValue != secondValue;
            }

            Assert.That(foundDifference, Is.True);
            Assert.That(first.CaptureState().Increment, Is.Not.EqualTo(second.CaptureState().Increment));
        }

        [Test]
        public void CapturedStateContinuesExactSequence()
        {
            DeterministicRandom uninterrupted = DeterministicRandom.Create(987654321UL, 123456UL);
            for (int index = 0; index < 137; index++)
            {
                uninterrupted.NextUInt32();
            }

            Pcg32State snapshot = uninterrupted.CaptureState();
            DeterministicRandom restored = DeterministicRandom.FromState(snapshot);

            for (int index = 0; index < 256; index++)
            {
                Assert.That(restored.NextUInt32(), Is.EqualTo(uninterrupted.NextUInt32()),
                    $"Continuation draw {index} drifted.");
            }
        }

        [Test]
        public void CapturingStateDoesNotConsumeADraw()
        {
            DeterministicRandom inspected = DeterministicRandom.Create(314159UL, 2718UL);
            DeterministicRandom untouched = DeterministicRandom.Create(314159UL, 2718UL);

            Pcg32State firstSnapshot = inspected.CaptureState();
            Pcg32State secondSnapshot = inspected.CaptureState();

            Assert.That(secondSnapshot, Is.EqualTo(firstSnapshot));
            Assert.That(inspected.NextUInt32(), Is.EqualTo(untouched.NextUInt32()));
        }

        [Test]
        public void StateRejectsDefaultAndEvenIncrement()
        {
            Assert.Throws<ArgumentException>(() => DeterministicRandom.FromState(default));
            Assert.Throws<ArgumentOutOfRangeException>(() => Pcg32State.Create(1UL, 2UL));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void NextInt32RejectsNonPositiveBound(int exclusiveMax)
        {
            DeterministicRandom random = DeterministicRandom.Create(1UL, 1UL);

            Pcg32State before = random.CaptureState();

            Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt32(exclusiveMax));
            Assert.That(random.CaptureState(), Is.EqualTo(before));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(31)]
        [TestCase(1000)]
        [TestCase(int.MaxValue)]
        public void NextInt32AlwaysStaysInsideExclusiveRange(int exclusiveMax)
        {
            DeterministicRandom random = DeterministicRandom.Create(ulong.MaxValue, 98765UL);

            for (int index = 0; index < 10000; index++)
            {
                int value = random.NextInt32(exclusiveMax);
                Assert.That(value, Is.GreaterThanOrEqualTo(0));
                Assert.That(value, Is.LessThan(exclusiveMax));
            }
        }

        [Test]
        public void BoundedDrawExercisesDeterministicRejectionPath()
        {
            Pcg32State beforeRejectedDraw = Pcg32State.Create(
                0xF8CB_5029_2A78_9B06UL,
                0x0000_0000_0000_006DUL);
            DeterministicRandom random = DeterministicRandom.FromState(beforeRejectedDraw);

            int value = random.NextInt32(1_073_741_825);

            Assert.That(value, Is.EqualTo(762_865_699));
            Assert.That(random.CaptureState().State, Is.EqualTo(0x2411_2B0A_D757_7F89UL),
                "The state proves that two below-threshold draws were rejected and a third draw was used.");
        }

        [Test]
        public void BoundOfOneStillConsumesExactlyOneDraw()
        {
            DeterministicRandom bounded = DeterministicRandom.Create(1234UL, 5678UL);
            DeterministicRandom raw = DeterministicRandom.Create(1234UL, 5678UL);

            Assert.That(bounded.NextInt32(1), Is.Zero);
            raw.NextUInt32();

            Assert.That(bounded.CaptureState(), Is.EqualTo(raw.CaptureState()));
        }

        [Test]
        public void ZeroAndMaximumInitialStatesRemainDeterministic()
        {
            DeterministicRandom zero = DeterministicRandom.Create(0UL, 0UL);
            DeterministicRandom zeroReplay = DeterministicRandom.Create(0UL, 0UL);
            DeterministicRandom maximum = DeterministicRandom.Create(
                ulong.MaxValue,
                Pcg32Algorithm.MaximumStreamSelector);
            DeterministicRandom maximumReplay = DeterministicRandom.Create(
                ulong.MaxValue,
                Pcg32Algorithm.MaximumStreamSelector);

            for (int index = 0; index < 128; index++)
            {
                Assert.That(zeroReplay.NextUInt32(), Is.EqualTo(zero.NextUInt32()));
                Assert.That(maximumReplay.NextUInt32(), Is.EqualTo(maximum.NextUInt32()));
            }
        }

        [Test]
        public void StreamSelectorRejectsHighBitAliasing()
        {
            const ulong firstAliasingValue = 0x8000_0000_0000_0000UL;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                DeterministicRandom.Create(42UL, firstAliasingValue));
        }

        [Test]
        public void StateValueUsesStableAlgorithmIdentityAndHexEncoding()
        {
            Pcg32State state = Pcg32State.Create(0x1234UL, 0x55UL);

            Assert.That(Pcg32Algorithm.Id, Is.EqualTo("pcg32-xsh-rr-64-32-v1"));
            Assert.That(
                state.ToString(),
                Is.EqualTo("pcg32-xsh-rr-64-32-v1:state=0000000000001234,increment=0000000000000055"));
        }
    }
}
