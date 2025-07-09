using System.Collections.Generic;
using UnityEngine;

namespace PNTG
{
    /// <summary>
    /// Input parameters for stroke validation
    /// </summary>
    public struct StrokeInputParams
    {
        public Vector2 position;
        public bool isPressed;
        public float deltaTime;
    }
    
    /// <summary>
    /// Result of stroke validation
    /// </summary>
    public struct StrokeValidationResult
    {
        public bool isValid;
        public bool isComplete;
        public float progress;
        public Vector2 closestPoint;
        public float distanceFromStroke;
    }
    
    /// <summary>
    /// Validates user input against stroke data
    /// </summary>
    public static class StrokeInputValidator
    {
        private static Dictionary<StrokeData, StrokeProgress> progressTrackers = new Dictionary<StrokeData, StrokeProgress>();
        
        private class StrokeProgress
        {
            public float currentParameter = 0f;
            public bool isTracking = false;
            public Vector2 lastValidPosition;
        }
        
        /// <summary>
        /// Validates input against a stroke and returns validation result
        /// </summary>
        public static StrokeValidationResult ValidateInput(StrokeData stroke, StrokeInputParams input)
        {
            if (stroke == null || stroke.ControlPoints.Length < 2)
            {
                return new StrokeValidationResult
                {
                    isValid = false,
                    isComplete = false,
                    progress = 0f
                };
            }
            
            // Get or create progress tracker
            if (!progressTrackers.TryGetValue(stroke, out var tracker))
            {
                tracker = new StrokeProgress();
                progressTrackers[stroke] = tracker;
            }
            
            var result = new StrokeValidationResult();
            
            // Find closest point on curve
            var (closestPoint, distance, parameter) = NURBSUtility.FindClosestPointOnCurve(
                stroke.ControlPoints,
                stroke.Degree,
                input.position,
                200
            );
            
            result.closestPoint = closestPoint;
            result.distanceFromStroke = distance;
            
            // Check if within wobble radius
            bool withinRadius = distance <= stroke.WobbleRadius;
            
            if (input.isPressed && withinRadius)
            {
                if (!tracker.isTracking)
                {
                    // Start tracking - check if starting near beginning
                    if (parameter < 0.1f)
                    {
                        tracker.isTracking = true;
                        tracker.currentParameter = parameter;
                        tracker.lastValidPosition = input.position;
                    }
                }
                else
                {
                    // Continue tracking - ensure forward progress
                    if (parameter >= tracker.currentParameter - 0.05f)
                    {
                        tracker.currentParameter = Mathf.Max(tracker.currentParameter, parameter);
                        tracker.lastValidPosition = input.position;
                    }
                }
                
                result.isValid = tracker.isTracking;
                result.progress = tracker.currentParameter;
                result.isComplete = tracker.currentParameter > 0.95f;
            }
            else if (!input.isPressed || !withinRadius)
            {
                // Reset if released or moved too far
                if (tracker.isTracking && !result.isComplete)
                {
                    tracker.isTracking = false;
                    tracker.currentParameter = 0f;
                }
                
                result.isValid = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Resets progress tracking for a stroke
        /// </summary>
        public static void ResetProgress(StrokeData stroke)
        {
            if (progressTrackers.ContainsKey(stroke))
            {
                progressTrackers.Remove(stroke);
            }
        }
        
        /// <summary>
        /// Clears all progress tracking
        /// </summary>
        public static void ClearAllProgress()
        {
            progressTrackers.Clear();
        }
    }
}