using NUnit.Framework;
using UnityEngine;

namespace PNTG.Editor.Tests
{
    /// <summary>
    /// Test to verify NURBS curve endpoints are handled correctly
    /// </summary>
    public class NURBSEndpointTest
    {
        [Test]
        public void EvaluateNURBS_AtT0_ReturnsFirstControlPoint()
        {
            // Arrange
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(100, 200), 1f),
                new NURBSControlPoint(new Vector2(300, 400), 1f),
                new NURBSControlPoint(new Vector2(500, 600), 1f)
            };
            int degree = 2;
            
            // Act
            var result = NURBSUtility.EvaluateNURBS(controlPoints, degree, 0f);
            
            // Assert
            Assert.AreEqual(controlPoints[0].position, result, "t=0 should return first control point");
        }
        
        [Test]
        public void EvaluateNURBS_AtT1_ReturnsLastControlPoint()
        {
            // Arrange
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(100, 200), 1f),
                new NURBSControlPoint(new Vector2(300, 400), 1f),
                new NURBSControlPoint(new Vector2(500, 600), 1f)
            };
            int degree = 2;
            
            // Act
            var result = NURBSUtility.EvaluateNURBS(controlPoints, degree, 1f);
            
            // Assert
            Assert.AreEqual(controlPoints[2].position, result, "t=1 should return last control point");
        }
        
        [Test]
        public void GenerateCurvePoints_FirstAndLastPoints_MatchControlPoints()
        {
            // Arrange
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(100, 200), 1f),
                new NURBSControlPoint(new Vector2(300, 400), 1f),
                new NURBSControlPoint(new Vector2(500, 600), 1f)
            };
            int degree = 2;
            int resolution = 50;
            
            // Act
            var points = NURBSUtility.GenerateCurvePoints(controlPoints, degree, resolution);
            
            // Assert
            Assert.AreEqual(resolution + 1, points.Count, "Should generate correct number of points");
            Assert.AreEqual(controlPoints[0].position, points[0], "First generated point should match first control point");
            Assert.AreEqual(controlPoints[2].position, points[points.Count - 1], "Last generated point should match last control point");
        }
        
        [Test]
        public void EvaluateNURBS_DoesNotReturnZero_ForValidInput()
        {
            // Arrange
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(100, 200), 1f),
                new NURBSControlPoint(new Vector2(300, 400), 1f),
                new NURBSControlPoint(new Vector2(500, 600), 1f)
            };
            int degree = 2;
            
            // Act & Assert
            for (int i = 0; i <= 10; i++)
            {
                float t = i / 10f;
                var result = NURBSUtility.EvaluateNURBS(controlPoints, degree, t);
                Assert.AreNotEqual(Vector2.zero, result, $"t={t} should not return Vector2.zero");
            }
        }
    }
}