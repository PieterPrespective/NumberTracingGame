using System;
using UnityEngine;

namespace PNTG
{
    /// <summary>
    /// Represents a single control point for a NURBS curve
    /// </summary>
    [Serializable]
    public struct NURBSControlPoint
    {
        public Vector2 position;
        public float weight;
        
        public NURBSControlPoint(Vector2 position, float weight = 1.0f)
        {
            this.position = position;
            this.weight = weight;
        }
    }
    
    /// <summary>
    /// Represents a single stroke in a number or shape
    /// </summary>
    [Serializable]
    public class StrokeData
    {
        [SerializeField] private NURBSControlPoint[] controlPoints;
        [SerializeField] private int degree = 3;
        [SerializeField] private float wobbleRadius = 20f;
        [SerializeField] private Color strokeColor = Color.black;
        [SerializeField] private Color progressColor = Color.green;
        [SerializeField] private Color completedColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange
        [SerializeField] private Color wobbleColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        
        public NURBSControlPoint[] ControlPoints => controlPoints;
        public int Degree => degree;
        public float WobbleRadius => wobbleRadius;
        public Color StrokeColor => strokeColor;
        public Color ProgressColor => progressColor;
        public Color CompletedColor => completedColor;
        public Color WobbleColor => wobbleColor;
        
        public StrokeData()
        {
            controlPoints = new NURBSControlPoint[0];
        }
        
        public void SetControlPoints(NURBSControlPoint[] points)
        {
            controlPoints = points;
        }
        
        public void UpdateControlPoint(int index, Vector2 position)
        {
            if (index >= 0 && index < controlPoints.Length)
            {
                controlPoints[index].position = position;
            }
        }
        
        public void AddControlPoint(Vector2 position, float weight = 1.0f)
        {
            var newPoints = new NURBSControlPoint[controlPoints.Length + 1];
            Array.Copy(controlPoints, newPoints, controlPoints.Length);
            newPoints[controlPoints.Length] = new NURBSControlPoint(position, weight);
            controlPoints = newPoints;
        }
        
        public void RemoveControlPoint(int index)
        {
            if (index >= 0 && index < controlPoints.Length && controlPoints.Length > degree + 1)
            {
                var newPoints = new NURBSControlPoint[controlPoints.Length - 1];
                int destIndex = 0;
                for (int i = 0; i < controlPoints.Length; i++)
                {
                    if (i != index)
                    {
                        newPoints[destIndex++] = controlPoints[i];
                    }
                }
                controlPoints = newPoints;
            }
        }
    }
}