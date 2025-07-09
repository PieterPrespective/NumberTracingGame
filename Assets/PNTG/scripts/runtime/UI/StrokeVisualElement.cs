using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PNTG
{
    /// <summary>
    /// Custom VisualElement that renders stroke overlays using Painter2D
    /// </summary>
    public class StrokeVisualElement : VisualElement
    {
        private StrokeData strokeData;
        private float progress = 0f;
        private bool showWobbleRadius = true;
        private bool showControlPoints = false;
        private bool isCompleted = false;
        private int selectedControlPoint = -1;
        private TraceCollection userTrace = new TraceCollection(2f);
        
        public StrokeData StrokeData
        {
            get => strokeData;
            set
            {
                strokeData = value;
                MarkDirtyRepaint();
            }
        }
        
        public float Progress
        {
            get => progress;
            set
            {
                progress = Mathf.Clamp01(value);
                MarkDirtyRepaint();
            }
        }
        
        public bool ShowWobbleRadius
        {
            get => showWobbleRadius;
            set
            {
                showWobbleRadius = value;
                MarkDirtyRepaint();
            }
        }
        
        public bool ShowControlPoints
        {
            get => showControlPoints;
            set
            {
                showControlPoints = value;
                MarkDirtyRepaint();
            }
        }
        
        public bool IsCompleted
        {
            get => isCompleted;
            set
            {
                isCompleted = value;
                MarkDirtyRepaint();
            }
        }
        
        public StrokeVisualElement()
        {
            generateVisualContent += OnGenerateVisualContent;
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
        }
        
        public void AddTracePoint(Vector2 position)
        {
            userTrace.AddPoint(position);
            MarkDirtyRepaint();
        }
        
        public void UpdateTrace()
        {
            userTrace.UpdateTrace();
            if (userTrace.TracePoints.Count > 0)
            {
                MarkDirtyRepaint();
            }
        }
        
        public void ClearTrace()
        {
            userTrace.Clear();
            MarkDirtyRepaint();
        }
        
        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (strokeData == null || strokeData.ControlPoints.Length < 2)
                return;
                
            var painter = mgc.painter2D;
            
            // Generate curve points
            var curvePoints = NURBSUtility.GenerateCurvePoints(
                strokeData.ControlPoints, 
                strokeData.Degree, 
                200
            );
            
            if (curvePoints.Count < 2)
                return;
            
            // Draw wobble radius outline if enabled and not completed
            if (showWobbleRadius && !isCompleted)
            {
                DrawWobbleRadius(painter, curvePoints, strokeData.WobbleRadius, strokeData.WobbleColor);
            }
            
            // Draw the main stroke
            DrawStroke(painter, curvePoints, strokeData.StrokeColor, 4f);
            
            // Draw completed overlay or progress overlay
            if (isCompleted)
            {
                DrawCompletedOverlay(painter, curvePoints, strokeData.CompletedColor, strokeData.WobbleRadius);
            }
            else if (progress > 0f)
            {
                DrawProgressOverlay(painter, curvePoints, progress, strokeData.ProgressColor, strokeData.WobbleRadius);
            }
            
            // Draw control points if in edit mode
            if (showControlPoints)
            {
                DrawControlPoints(painter, strokeData.ControlPoints);
            }
            
            // Draw user trace
            DrawUserTrace(painter, userTrace.TracePoints);
        }
        
        private void DrawWobbleRadius(Painter2D painter, List<Vector2> points, float radius, Color color)
        {
            painter.strokeColor = Color.clear;
            painter.fillColor = color;
            
            // Create outline path with wobble radius
            painter.BeginPath();
            
            // Forward pass
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];
                Vector2 normal = GetNormal(points, i);
                Vector2 offset = normal * radius;
                
                if (i == 0)
                    painter.MoveTo(point + offset);
                else
                    painter.LineTo(point + offset);
            }
            
            // Backward pass
            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector2 point = points[i];
                Vector2 normal = GetNormal(points, i);
                Vector2 offset = normal * radius;
                
                painter.LineTo(point - offset);
            }
            
            painter.ClosePath();
            painter.Fill();
        }
        
        private void DrawStroke(Painter2D painter, List<Vector2> points, Color color, float width)
        {
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            
            painter.BeginPath();
            painter.MoveTo(points[0]);
            
            for (int i = 1; i < points.Count; i++)
            {
                painter.LineTo(points[i]);
            }
            
            painter.Stroke();
        }
        
        private void DrawProgressOverlay(Painter2D painter, List<Vector2> points, float progress, Color color, float radius)
        {
            if (progress <= 0f)
                return;
                
            int progressIndex = Mathf.FloorToInt((points.Count - 1) * progress);
            
            painter.strokeColor = Color.clear;
            painter.fillColor = color;
            
            painter.BeginPath();
            
            // Forward pass up to progress
            for (int i = 0; i <= progressIndex; i++)
            {
                Vector2 point = points[i];
                Vector2 normal = GetNormal(points, i);
                Vector2 offset = normal * radius;
                
                if (i == 0)
                    painter.MoveTo(point + offset);
                else
                    painter.LineTo(point + offset);
            }
            
            // Backward pass
            for (int i = progressIndex; i >= 0; i--)
            {
                Vector2 point = points[i];
                Vector2 normal = GetNormal(points, i);
                Vector2 offset = normal * radius;
                
                painter.LineTo(point - offset);
            }
            
            painter.ClosePath();
            painter.Fill();
        }
        
        private void DrawControlPoints(Painter2D painter, NURBSControlPoint[] controlPoints)
        {
            for (int i = 0; i < controlPoints.Length; i++)
            {
                Vector2 pos = controlPoints[i].position;
                
                painter.fillColor = (i == selectedControlPoint) ? Color.yellow : Color.blue;
                painter.strokeColor = Color.black;
                painter.lineWidth = 2f;
                
                painter.BeginPath();
                painter.Arc(pos, 8f, 0f, 360f * Mathf.Deg2Rad);
                painter.Fill();
                painter.Stroke();
            }
        }
        
        private Vector2 GetNormal(List<Vector2> points, int index)
        {
            Vector2 tangent;
            
            if (index == 0)
            {
                tangent = (points[1] - points[0]).normalized;
            }
            else if (index == points.Count - 1)
            {
                tangent = (points[index] - points[index - 1]).normalized;
            }
            else
            {
                tangent = (points[index + 1] - points[index - 1]).normalized;
            }
            
            return new Vector2(-tangent.y, tangent.x);
        }
        
        public int GetControlPointAtPosition(Vector2 position)
        {
            if (strokeData == null || !showControlPoints)
                return -1;
                
            for (int i = 0; i < strokeData.ControlPoints.Length; i++)
            {
                if (Vector2.Distance(position, strokeData.ControlPoints[i].position) < 10f)
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        public void SelectControlPoint(int index)
        {
            selectedControlPoint = index;
            MarkDirtyRepaint();
        }
        
        private void DrawCompletedOverlay(Painter2D painter, List<Vector2> points, Color color, float radius)
        {
            painter.strokeColor = Color.clear;
            painter.fillColor = color;
            
            // Create outline path with wobble radius - filled with completed color
            painter.BeginPath();
            
            // Forward pass
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];
                Vector2 normal = GetNormal(points, i);
                Vector2 offset = normal * radius;
                
                if (i == 0)
                    painter.MoveTo(point + offset);
                else
                    painter.LineTo(point + offset);
            }
            
            // Backward pass
            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector2 point = points[i];
                Vector2 normal = GetNormal(points, i);
                Vector2 offset = normal * radius;
                
                painter.LineTo(point - offset);
            }
            
            painter.ClosePath();
            painter.Fill();
        }
        
        private void DrawUserTrace(Painter2D painter, List<TracePoint> tracePoints)
        {
            if (tracePoints.Count < 2)
                return;
                
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            painter.lineWidth = 3f;
            
            // Draw trace segments with fading alpha
            for (int i = 0; i < tracePoints.Count - 1; i++)
            {
                var startPoint = tracePoints[i];
                var endPoint = tracePoints[i + 1];
                
                // Use average alpha for the segment
                float alpha = (startPoint.alpha + endPoint.alpha) * 0.5f;
                painter.strokeColor = new Color(1f, 0.5f, 0f, alpha); // Orange trace
                
                painter.BeginPath();
                painter.MoveTo(startPoint.position);
                painter.LineTo(endPoint.position);
                painter.Stroke();
            }
        }
    }
}