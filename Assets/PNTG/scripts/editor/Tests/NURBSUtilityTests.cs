using NUnit.Framework;
using UnityEngine;

namespace PNTG.Editor.Tests
{
    /// <summary>
    /// Unit tests for NURBS utility functions
    /// </summary>
    public class NURBSUtilityTests
    {
        [Test]
        public void EvaluateNURBS_WithValidControlPoints_ReturnsValidPosition()
        {
            // Arrange
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(0, 0), 1f),
                new NURBSControlPoint(new Vector2(100, 0), 1f),
                new NURBSControlPoint(new Vector2(100, 100), 1f),
                new NURBSControlPoint(new Vector2(0, 100), 1f)
            };
            int degree = 3;
            
            // Act
            var startPoint = NURBSUtility.EvaluateNURBS(controlPoints, degree, 0f);
            var endPoint = NURBSUtility.EvaluateNURBS(controlPoints, degree, 1f);
            var midPoint = NURBSUtility.EvaluateNURBS(controlPoints, degree, 0.5f);
            
            // Assert
            Assert.AreEqual(controlPoints[0].position, startPoint, "Start point should match first control point");
            Assert.AreEqual(controlPoints[3].position, endPoint, "End point should match last control point");
            Assert.IsTrue(midPoint.magnitude > 0, "Mid point should be valid");
        }
        
        [Test]
        public void GenerateCurvePoints_WithValidInput_ReturnsCorrectNumberOfPoints()
        {
            // Arrange
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(0, 0), 1f),
                new NURBSControlPoint(new Vector2(100, 100), 1f),
                new NURBSControlPoint(new Vector2(200, 0), 1f)
            };
            int degree = 2;
            int resolution = 50;
            
            // Act
            var points = NURBSUtility.GenerateCurvePoints(controlPoints, degree, resolution);
            
            // Assert
            Assert.AreEqual(resolution + 1, points.Count, "Should generate resolution + 1 points");
            Assert.AreEqual(controlPoints[0].position, points[0], "First point should match first control point");
            Assert.AreEqual(controlPoints[2].position, points[points.Count - 1], "Last point should match last control point");
        }
        
        [Test]
        public void IsPointWithinWobbleRadius_WithPointOnCurve_ReturnsTrue()
        {
            // Arrange
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(0, 0), 1f),
                new NURBSControlPoint(new Vector2(100, 0), 1f)
            };
            int degree = 1;
            float wobbleRadius = 10f;
            var testPoint = new Vector2(50, 0); // Point on the line
            
            // Act
            bool result = NURBSUtility.IsPointWithinWobbleRadius(controlPoints, degree, testPoint, wobbleRadius);
            
            // Assert
            Assert.IsTrue(result, "Point on curve should be within wobble radius");
        }
        
        [Test]
        public void IsPointWithinWobbleRadius_WithPointOutsideRadius_ReturnsFalse()
        {
            // Arrange
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(0, 0), 1f),
                new NURBSControlPoint(new Vector2(100, 0), 1f)
            };
            int degree = 1;
            float wobbleRadius = 10f;
            var testPoint = new Vector2(50, 20); // Point far from the line
            
            // Act
            bool result = NURBSUtility.IsPointWithinWobbleRadius(controlPoints, degree, testPoint, wobbleRadius);
            
            // Assert
            Assert.IsFalse(result, "Point outside wobble radius should return false");
        }
    }
}