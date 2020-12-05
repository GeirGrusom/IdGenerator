using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdGenerator.Tests
{
    public readonly partial struct Degrees : IEquatable<Degrees>, IComparable<Degrees>
    {
        private readonly double value;

        private bool Validate()
        {
            return value is >= 0 and < 360;
        }

        public static implicit operator Radians(Degrees input) => input.value * (Math.PI / 180);
    }

    public readonly partial struct Radians : IEquatable<Radians>, IComparable<Radians>
    {
        private readonly double value;

        private bool Validate()
        {
            return value is >= 0 and < 2 * Math.PI;
        }

        public static implicit operator Degrees(Radians input) => input.value / (Math.PI / 180);
    }

    public class ComparerGeneratedCodeTests
    {
        [Test]
        public void GreaterThan()
        {
            var a = (Degrees)123;

            var b = (Degrees)124;

            Assert.That(b > a);
        }

        [Test]
        public void GreaterThanOrEqual()
        {
            var a = (Degrees)123;

            var b = (Degrees)123;

            Assert.That(b >= a);
        }

        [Test]
        public void LessThan()
        {
            var a = (Degrees)124;

            var b = (Degrees)123;

            Assert.That(b < a);
        }

        [Test]
        public void LessThanOrEqual()
        {
            var a = (Degrees)123;

            var b = (Degrees)123;

            Assert.That(b <= a);
        }

        [Test]
        public void ValidationFails_ThrowsFormatException()
        {
            Assert.That(() => (Degrees)(-1.0), Throws.InstanceOf<ArgumentException>());
        }
    }
}
