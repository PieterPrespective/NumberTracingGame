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
        public float visualProgress; // Progress capped by checkpoints for visualization
        public Vector2 closestPoint;
        public float distanceFromStroke;
    }
    
    /// <summary>
    /// Validates user input against stroke data using a checkpoint system
    /// </summary>
    public static class StrokeInputValidator
    {
        private static Dictionary<StrokeData, StrokeProgress> progressTrackers = new Dictionary<StrokeData, StrokeProgress>();
        
        private class StrokeProgress
        {
            public float currentParameter = 0f;
            public bool isTracking = false;
            public Vector2 lastValidPosition;
            public List<Checkpoint> checkpoints = new List<Checkpoint>();
            public int lastClearedCheckpoint = -1;
        }
        
        private class Checkpoint
        {
            public float parameter;
            public Vector2 position;
            public bool cleared;
            
            public Checkpoint(float param, Vector2 pos)
            {
                parameter = param;
                position = pos;
                cleared = false;
            }
        }
        
        /// <summary>
        /// Generates checkpoints along the stroke based on the checkpoint interval
        /// </summary>
        private static List<Checkpoint> GenerateCheckpoints(StrokeData stroke)
        {
            var checkpoints = new List<Checkpoint>();
            
            float interval = stroke.CheckpointInterval;
            if (interval <= 0 || interval >= 1)
            {
                Debug.LogWarning($"Invalid checkpoint interval: {interval}. Using default 0.1");
                interval = 0.1f;
            }
            
            // Generate checkpoints at regular parameter intervals
            for (float t = interval; t < 1.0f; t += interval)
            {
                var position = NURBSUtility.EvaluateNURBS(stroke.ControlPoints, stroke.Degree, t);
                checkpoints.Add(new Checkpoint(t, position));
            }
            
            // Always add a checkpoint near the end (at 95%)
            if (checkpoints.Count == 0 || checkpoints[checkpoints.Count - 1].parameter < 0.95f)
            {
                var endPosition = NURBSUtility.EvaluateNURBS(stroke.ControlPoints, stroke.Degree, 0.95f);
                checkpoints.Add(new Checkpoint(0.95f, endPosition));
            }
            
            return checkpoints;
        }
        
        /// <summary>
        /// Validates input against a stroke and returns validation result
        /// </summary>
        public static StrokeValidationResult ValidateInput(StrokeData stroke, StrokeInputParams input)
        {
            if (stroke == null || stroke.ControlPoints.Length < 2)
            {
                Debug.LogWarning("Invalid stroke data provided for validation.");
                return new StrokeValidationResult
                {
                    isValid = false,
                    isComplete = false,
                    progress = 0f,
                    visualProgress = 0f
                };
            }
            
            // Get or create progress tracker
            if (!progressTrackers.TryGetValue(stroke, out var tracker))
            {
                tracker = new StrokeProgress();
                tracker.checkpoints = GenerateCheckpoints(stroke);
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
                        tracker.lastClearedCheckpoint = -1;
                        
                        // Reset all checkpoints
                        foreach (var checkpoint in tracker.checkpoints)
                        {
                            checkpoint.cleared = false;
                        }
                    }
                }
                else
                {
                    // Continue tracking - check for checkpoint clearing
                    
                    // Update checkpoints - clear any that are within wobble radius
                    for (int i = 0; i < tracker.checkpoints.Count; i++)
                    {
                        var checkpoint = tracker.checkpoints[i];
                        if (!checkpoint.cleared)
                        {
                            float checkpointDistance = Vector2.Distance(input.position, checkpoint.position);
                            if (checkpointDistance <= stroke.WobbleRadius)
                            {
                                // Only clear if it's the next expected checkpoint or we're past it
                                if (i <= tracker.lastClearedCheckpoint + 1 || parameter >= checkpoint.parameter)
                                {
                                    checkpoint.cleared = true;
                                    tracker.lastClearedCheckpoint = Mathf.Max(tracker.lastClearedCheckpoint, i);
                                    Debug.Log($"Cleared checkpoint {i} at parameter {checkpoint.parameter:F3}");
                                }
                            }
                        }
                    }
                    
                    // Allow forward progress or slight backward movement
                    if (parameter >= tracker.currentParameter - 0.05f)
                    {
                        tracker.currentParameter = Mathf.Max(tracker.currentParameter, parameter);
                        tracker.lastValidPosition = input.position;
                    }
                }
                
                // Calculate progress based on cleared checkpoints
                int clearedCount = 0;
                float nextCheckpointParameter = 1.0f; // Default to end if all cleared
                
                // Find the next uncleared checkpoint to cap visual progress
                for (int i = 0; i < tracker.checkpoints.Count; i++)
                {
                    var checkpoint = tracker.checkpoints[i];
                    if (checkpoint.cleared)
                    {
                        clearedCount++;
                    }
                    else
                    {
                        // Found the next uncleared checkpoint
                        nextCheckpointParameter = checkpoint.parameter;
                        break;
                    }
                }
                
                Debug.Log($"tracking: {tracker.isTracking}, parameter: {tracker.currentParameter:F3}, cleared: {clearedCount}/{tracker.checkpoints.Count}, next checkpoint: {nextCheckpointParameter:F3}");

                result.isValid = tracker.isTracking;
                result.progress = tracker.currentParameter;
                
                // Visual progress is capped by the next uncleared checkpoint
                result.visualProgress = Mathf.Min(tracker.currentParameter, nextCheckpointParameter);
                
                // Complete only if near end AND all checkpoints cleared
                bool allCheckpointsCleared = clearedCount == tracker.checkpoints.Count;
                result.isComplete = tracker.currentParameter > 0.95f && allCheckpointsCleared;
            }
            else if (!input.isPressed || !withinRadius)
            {
                // Reset if released or moved too far
                if (tracker.isTracking && !result.isComplete)
                {
                    tracker.isTracking = false;
                    tracker.currentParameter = 0f;
                    tracker.lastClearedCheckpoint = -1;
                    
                    // Reset all checkpoints
                    foreach (var checkpoint in tracker.checkpoints)
                    {
                        checkpoint.cleared = false;
                    }
                }
                Debug.Log("not tracking, reset progress");
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