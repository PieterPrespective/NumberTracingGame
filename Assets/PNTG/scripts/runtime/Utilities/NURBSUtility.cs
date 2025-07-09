using System.Collections.Generic;
using UnityEngine;

namespace PNTG
{
    /// <summary>
    /// Utility class for NURBS curve calculations and operations
    /// </summary>
    public static class NURBSUtility
    {
        /// <summary>
        /// Evaluates a NURBS curve at parameter t
        /// </summary>
        public static Vector2 EvaluateNURBS(NURBSControlPoint[] controlPoints, int degree, float t)
        {
            if (controlPoints == null || controlPoints.Length < degree + 1)
                return Vector2.zero;
                
            // Handle boundary cases
            t = Mathf.Clamp01(t);
            if (t == 0f)
                return controlPoints[0].position;
            if (t == 1f)
                return controlPoints[controlPoints.Length - 1].position;
                
            int n = controlPoints.Length - 1;
            float[] knots = GenerateUniformKnotVector(n, degree);
            
            Vector2 result = Vector2.zero;
            float weightSum = 0f;
            
            for (int i = 0; i <= n; i++)
            {
                float basis = BSplineBasis(i, degree, t, knots);
                float weight = controlPoints[i].weight;
                result += controlPoints[i].position * basis * weight;
                weightSum += basis * weight;
            }
            
            if (weightSum > 0)
                result /= weightSum;
            else
                return controlPoints[controlPoints.Length - 1].position; // Fallback to last control point
                
            return result;
        }
        
        /// <summary>
        /// Generates a uniform knot vector for the B-spline basis
        /// </summary>
        private static float[] GenerateUniformKnotVector(int n, int degree)
        {
            int m = n + degree + 1;
            float[] knots = new float[m + 1];
            
            for (int i = 0; i <= degree; i++)
                knots[i] = 0f;
                
            for (int i = m - degree; i <= m; i++)
                knots[i] = 1f;
                
            for (int i = degree + 1; i < m - degree; i++)
                knots[i] = (float)(i - degree) / (n - degree + 1);
                
            return knots;
        }
        
        /// <summary>
        /// Computes the B-spline basis function
        /// </summary>
        private static float BSplineBasis(int i, int p, float t, float[] knots)
        {
            if (p == 0)
            {
                return (t >= knots[i] && t < knots[i + 1]) ? 1f : 0f;
            }
            
            float left = 0f;
            if (knots[i + p] != knots[i])
            {
                left = (t - knots[i]) / (knots[i + p] - knots[i]) * BSplineBasis(i, p - 1, t, knots);
            }
            
            float right = 0f;
            if (knots[i + p + 1] != knots[i + 1])
            {
                right = (knots[i + p + 1] - t) / (knots[i + p + 1] - knots[i + 1]) * BSplineBasis(i + 1, p - 1, t, knots);
            }
            
            return left + right;
        }
        
        /// <summary>
        /// Generates points along the NURBS curve for rendering
        /// </summary>
        public static List<Vector2> GenerateCurvePoints(NURBSControlPoint[] controlPoints, int degree, int resolution = 100)
        {
            var points = new List<Vector2>(resolution + 1);
            
            if (controlPoints == null || controlPoints.Length < degree + 1)
                return points;
                
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                points.Add(EvaluateNURBS(controlPoints, degree, t));
            }
            
            return points;
        }
        
        /// <summary>
        /// Finds the closest point on the NURBS curve to a given point
        /// </summary>
        public static (Vector2 closestPoint, float distance, float parameter) FindClosestPointOnCurve(
            NURBSControlPoint[] controlPoints, int degree, Vector2 point, int searchResolution = 100)
        {
            float minDistance = float.MaxValue;
            Vector2 closestPoint = Vector2.zero;
            float closestParameter = 0f;
            
            for (int i = 0; i <= searchResolution; i++)
            {
                float t = i / (float)searchResolution;
                Vector2 curvePoint = EvaluateNURBS(controlPoints, degree, t);
                float distance = Vector2.Distance(point, curvePoint);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = curvePoint;
                    closestParameter = t;
                }
            }
            
            return (closestPoint, minDistance, closestParameter);
        }
        
        /// <summary>
        /// Checks if a point is within the wobble radius of the curve
        /// </summary>
        public static bool IsPointWithinWobbleRadius(NURBSControlPoint[] controlPoints, int degree, 
            Vector2 point, float wobbleRadius)
        {
            var (_, distance, _) = FindClosestPointOnCurve(controlPoints, degree, point);
            return distance <= wobbleRadius;
        }
    }
}