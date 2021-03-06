using NUnit.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Threading;

namespace IdGenerator.Tests
{
    public readonly partial struct IntTest : IEquatable<IntTest>
    {
        private readonly int value;
    }

    public readonly partial struct GuidTest : IEquatable<GuidTest>
    {
        private readonly Guid value;
    }

    public readonly partial struct DecimalTest : IEquatable<DecimalTest>
    {
        private readonly decimal value;
    }

    public readonly partial struct StringTest : IEquatable<StringTest>
    {
        private readonly string value;
    }

    public readonly partial struct TupleTest : IEquatable<TupleTest>
    {
        private readonly (StringTest, IntTest) value;
    }

    public readonly partial struct DateTimeTest : IEquatable<DateTimeTest>
    {
        private readonly DateTime value;
    }

    public readonly partial struct DateTimeOffsetTest : IEquatable<DateTimeOffsetTest>
    {
        private readonly DateTimeOffset value;
    }

    public readonly partial struct Vector128Test : IEquatable<Vector128Test>
    {
        private readonly Vector128<byte> value;
    }

    public readonly partial struct Vector256Test : IEquatable<Vector256Test>
    {
        private readonly Vector256<byte> value;
    }

    public readonly partial struct UriTest : IEquatable<UriTest>
    {
        private readonly Uri value;
    }

    public class GeneratedCodeTests
    {
        [Test]
        public void GetHashCode_ReturnsInnerTypesHashCode()
        {
            // Arrange
            int value = 123;
            IntTest subject = value;

            // Act
            // Assert
            Assert.That(subject.GetHashCode(), Is.EqualTo(value.GetHashCode()));
        }

        [Test]
        public void ToString_ReturnsInnerTypesString()
        {
            // Arrange
            Guid value = Guid.NewGuid();
            GuidTest subject = value;

            // Act
            // Assert
            Assert.That(subject.ToString(), Is.EqualTo(value.ToString()));
        }

        [Test]
        public void ToString_IsCultureInvariant()
        {
            // Arrange
            var culture = new CultureInfo(CultureInfo.InvariantCulture.Name);
            culture.NumberFormat.NumberDecimalSeparator = ";";
            culture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;
            culture.NumberFormat.NativeDigits = new[] { "\u0660", "\u0661", "\u0662", "\u0663", "\u0664", "\u0665", "\u0666", "\u0667", "\u0668", "\u0669" };
            Thread.CurrentThread.CurrentCulture = culture;

            decimal i = 123.456m;
            DecimalTest subject = i;

            // Act
            // Assert
            Assert.That(subject.ToString(), Is.EqualTo("123.456"));
        }

        [Test]
        public void Equals_AreEqual_ReturnsTrue()
        {
            // Arrange
            var a = (IntTest)1;
            var b = (IntTest)1;

            // Act
            // Assert
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void Equals_Obj_WrongType_ReturnsFalse()
        {
            // Arrange
            var a = (IntTest)1;
            var b = 1;

            // Act
            // Assert
            Assert.That(a, Is.Not.EqualTo(b));
        }

        [Test]
        public void Equals_String_SameValue_ReturnsTrue()
        {
            // Arrange
            var a = (StringTest)123.0.ToString();
            var b = (StringTest)123.ToString();

            // Act
            // Assert
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void Equals_String_DifferentValues_ReturnsTrue()
        {
            // Arrange
            var a = (StringTest)123.0.ToString();
            var b = (StringTest)1234.ToString();

            // Act
            // Assert
            Assert.That(a, Is.Not.EqualTo(b));
        }

        [Test]
        public void Equals_Obj_Null_ReturnsFalse()
        {
            // Arrange
            var a = (IntTest)1;

            // Act
            // Assert
            Assert.That(a.Equals(null!), Is.False);
        }

        [Test]
        public void OpEq_AreEqual_ReturnsTrue()
        {
            // Arrange
            var a = (IntTest)1;
            var b = (IntTest)1;

            // Act
            // Assert
            Assert.That(a == b);
        }

        [Test]
        public void OpNotEq_AreEqual_ReturnsTrue()
        {
            // Arrange
            var a = (IntTest)1;
            var b = (IntTest)2;

            // Act
            // Assert
            Assert.That(a != b);
        }

        [Test]
        public void ToString_DateTime_Utc_ReturnsIso8601()
        {
            // Arrange
            var value = (DateTimeTest)new DateTime(2001, 03, 19, 12, 31, 24, DateTimeKind.Utc);

            // Act
            var result = value.ToString();

            // Assert
            Assert.That(result, Is.EqualTo("2001-03-19T12:31:24.0000000Z"));
        }

        [Test]
        public void Equals_CompareToDefaultNullable_String_AreNotEqual()
        {
            // Arrange
            var value = (StringTest)"abc123";

            // Act
            Assert.That(value.Equals((object)default(StringTest)), Is.False);
        }

        [Test]
        public void Equals_CompareToDefaultNullable_Uri_AreNotEqual()
        {
            // Arrange
            var value = (UriTest)new Uri("https://localhost");
            UriTest uri = default;

            // Act
            Assert.That(value.Equals((object)uri), Is.False);
        }

        [Test]
        public void Equals_CompareToDefault_Uri_AreNotEqual()
        {
            // Arrange
            var value = (UriTest)new Uri("https://localhost");
            UriTest uri = default;

            // Act
            Assert.That(uri.Equals(value), Is.False);
        }

        [Test]
        public void ToString_DateTimeOffset_ReturnsIso8601()
        {
            // Arrange
            var value = (DateTimeOffsetTest)new DateTimeOffset(2001, 03, 19, 12, 31, 24, 0, TimeSpan.FromHours(1));

            // Act
            var result = value.ToString();

            // Assert
            Assert.That(result, Is.EqualTo("2001-03-19T12:31:24.0000000+01:00"));
        }
    }
}