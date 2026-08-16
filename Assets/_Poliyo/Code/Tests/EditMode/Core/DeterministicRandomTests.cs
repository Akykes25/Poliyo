using System;
using NUnit.Framework;

namespace Poliyo.Core.EditModeTests
{
public sealed class DeterministicRandomTests
{
    [Test]
    public void NextInt_WithValidRange_ReturnsValueInsideRange()
    {
        var random = new DeterministicRandom(123UL);

        for (var index = 0; index < 100; index++)
        {
            var value = random.NextInt(-5, 8);

            Assert.That(value, Is.GreaterThanOrEqualTo(-5));
            Assert.That(value, Is.LessThan(8));
        }
    }

    [Test]
    public void NextInt_WithInvalidRange_ThrowsArgumentOutOfRangeException()
    {
        var random = new DeterministicRandom(123UL);

        Assert.That(
            () => random.NextInt(4, 4),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void NextDouble_ReturnsValueFromZeroInclusiveToOneExclusive()
    {
        var random = new DeterministicRandom(456UL);

        for (var index = 0; index < 100; index++)
        {
            var value = random.NextDouble();

            Assert.That(value, Is.GreaterThanOrEqualTo(0.0));
            Assert.That(value, Is.LessThan(1.0));
        }
    }
}

}
