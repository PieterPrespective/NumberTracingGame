using System.Collections.Generic;
using UnityEngine;

namespace PNTG
{
    /// <summary>
    /// Represents a single point in the user's trace
    /// </summary>
    [System.Serializable]
    public struct TracePoint
    {
        public Vector2 position;
        public float timestamp;
        public float alpha;
        
        public TracePoint(Vector2 position, float timestamp)
        {
            this.position = position;
            this.timestamp = timestamp;
            this.alpha = 1f;
        }
    }
    
    /// <summary>
    /// Manages a collection of trace points with fading
    /// </summary>
    public class TraceCollection
    {
        private List<TracePoint> tracePoints = new List<TracePoint>();
        private float fadeTime = 2f;
        
        public List<TracePoint> TracePoints => tracePoints;
        public float FadeTime => fadeTime;
        
        public TraceCollection(float fadeTime = 2f)
        {
            this.fadeTime = fadeTime;
        }
        
        public void AddPoint(Vector2 position)
        {
            tracePoints.Add(new TracePoint(position, Time.time));
        }
        
        public void UpdateTrace()
        {
            float currentTime = Time.time;
            
            // Update alpha values and remove old points
            for (int i = tracePoints.Count - 1; i >= 0; i--)
            {
                var point = tracePoints[i];
                float age = currentTime - point.timestamp;
                
                if (age > fadeTime)
                {
                    tracePoints.RemoveAt(i);
                }
                else
                {
                    point.alpha = 1f - (age / fadeTime);
                    tracePoints[i] = point;
                }
            }
        }
        
        public void Clear()
        {
            tracePoints.Clear();
        }
    }
}