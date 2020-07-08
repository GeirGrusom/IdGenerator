using NUnit.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
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
            var s = i.ToString();
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
        public void Equals_Obj_Null_ReturnsFalse()
        {
            // Arrange
            var a = (IntTest)1;

            // Act
            // Assert
            Assert.That(a.Equals(null), Is.False);
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
    }
}