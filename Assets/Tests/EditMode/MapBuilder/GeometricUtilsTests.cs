using NUnit.Framework;
using UnityEngine;
using Assets.Prefabs.MapBuilder;

namespace Assets.Tests.EditMode
{
    public class GeometricUtilsTests
    {
        [TearDown]
        public void TearDown()
        {
            // Explicit teardown for symmetry with other edit-mode tests.
            // These tests are pure and don't allocate Unity objects.
        }

        [Test]
        public void ObtuseAngle_ReturnsTrue_WhenAngleIsObtuse()
        {
            // Arrange
            Vector2 A = new Vector2(0, 0);
            Vector2 B = new Vector2(1, 0);
            Vector2 other = new Vector2(-1, 1); // This creates an obtuse angle at point A (Dot product < 0)

            // Act
            bool result = GeometricUtils.ThereAreObtuseAnglesNearAB(A, B, other);

            // Assert
            Assert.IsTrue(result, "Should return true for an obtuse angle at A");
        }

        [Test]
        public void AcuteAngle_ReturnsFalse_WhenAllAnglesAreAcute()
        {
            // Arrange
            Vector2 A = new Vector2(0, 0);
            Vector2 B = new Vector2(2, 0);
            Vector2 other = new Vector2(1, 1); // Perfect isosceles acute triangle

            // Act
            bool result = GeometricUtils.ThereAreObtuseAnglesNearAB(A, B, other);

            // Assert
            Assert.IsFalse(result, "Should return false for acute angles");
        }

        [Test]
        public void RightAngle_ReturnsFalse_WhenAngleIs90Degrees()
        {
            // Arrange
            Vector2 A = new Vector2(0, 0);
            Vector2 B = new Vector2(1, 0);
            Vector2 other = new Vector2(0, 1); // Right angle at A (Dot product = 0)

            // Act
            bool result = GeometricUtils.ThereAreObtuseAnglesNearAB(A, B, other);

            // Assert
            Assert.IsFalse(result, "Should return false for a perfect right angle");
        }
    }
}
