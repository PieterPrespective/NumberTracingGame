using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace PNTG
{
    /// <summary>
    /// Allows editing of stroke control points during play mode
    /// </summary>
    public class RuntimeControlPointEditor : MonoBehaviour
    {
        [SerializeField] private NumberConfiguration targetConfiguration;
        [SerializeField] private int strokeIndex = 0;
        [SerializeField] private KeyCode toggleEditModeKey = KeyCode.E;
        [SerializeField] private KeyCode addPointKey = KeyCode.A;
        [SerializeField] private KeyCode deletePointKey = KeyCode.Delete;
        
        private VisualElement strokeOverlay;
        private StrokeVisualElement strokeVisual;
        private bool isEditMode = false;
        private int selectedControlPoint = -1;
        private bool isDragging = false;
        
        public bool IsEditMode => isEditMode;
        
        private void Start()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                strokeOverlay = root.Q<VisualElement>("StrokeOverlay");
                
                if (strokeOverlay != null && targetConfiguration != null)
                {
                    SetupStrokeVisual();
                }
            }
        }
        
        private void SetupStrokeVisual()
        {
            if (strokeIndex >= 0 && strokeIndex < targetConfiguration.Strokes.Length)
            {
                strokeVisual = new StrokeVisualElement
                {
                    StrokeData = targetConfiguration.Strokes[strokeIndex],
                    ShowWobbleRadius = true,
                    ShowControlPoints = false,
                    Progress = 0f
                };
                
                strokeOverlay.Clear();
                strokeOverlay.Add(strokeVisual);
            }
        }
        
        private void Update()
        {
            if (strokeVisual == null || targetConfiguration == null)
                return;
                
            // Toggle edit mode
            if (Keyboard.current[Key.E].wasPressedThisFrame)
            {
                ToggleEditMode();
            }
            
            if (!isEditMode)
                return;
                
            HandleEditModeInput();
        }
        
        private void ToggleEditMode()
        {
            isEditMode = !isEditMode;
            strokeVisual.ShowControlPoints = isEditMode;
            
            if (!isEditMode)
            {
                selectedControlPoint = -1;
                strokeVisual.SelectControlPoint(-1);
                SaveConfiguration();
            }
            
            Debug.Log($"Edit mode: {(isEditMode ? "ON" : "OFF")}");
        }
        
        private void HandleEditModeInput()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;
                
            var mousePos = mouse.position.ReadValue();
            var rect = strokeOverlay.worldBound;
            
            // Convert to UI space
            var uiPosition = new Vector2(
                (mousePos.x - rect.x) / rect.width * strokeOverlay.resolvedStyle.width,
                (Screen.height - mousePos.y - rect.y) / rect.height * strokeOverlay.resolvedStyle.height
            );
            
            // Handle mouse click
            if (mouse.leftButton.wasPressedThisFrame)
            {
                selectedControlPoint = strokeVisual.GetControlPointAtPosition(uiPosition);
                strokeVisual.SelectControlPoint(selectedControlPoint);
                
                if (selectedControlPoint >= 0)
                {
                    isDragging = true;
                }
            }
            
            // Handle dragging
            if (isDragging && mouse.leftButton.isPressed && selectedControlPoint >= 0)
            {
                var strokeData = targetConfiguration.Strokes[strokeIndex];
                strokeData.UpdateControlPoint(selectedControlPoint, uiPosition);
                strokeVisual.StrokeData = strokeData;
            }
            
            // Release drag
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }
            
            // Add control point
            if (Keyboard.current[Key.A].wasPressedThisFrame)
            {
                var strokeData = targetConfiguration.Strokes[strokeIndex];
                strokeData.AddControlPoint(uiPosition);
                strokeVisual.StrokeData = strokeData;
                Debug.Log("Added control point");
            }
            
            // Delete selected control point
            if (Keyboard.current[Key.Delete].wasPressedThisFrame && selectedControlPoint >= 0)
            {
                var strokeData = targetConfiguration.Strokes[strokeIndex];
                strokeData.RemoveControlPoint(selectedControlPoint);
                strokeVisual.StrokeData = strokeData;
                selectedControlPoint = -1;
                strokeVisual.SelectControlPoint(-1);
                Debug.Log("Removed control point");
            }
        }
        
        private void SaveConfiguration()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(targetConfiguration);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("Configuration saved");
#endif
        }
        
        private void OnGUI()
        {
            if (!isEditMode)
                return;
                
            GUI.Box(new Rect(10, 10, 300, 100), "Control Point Editor");
            GUI.Label(new Rect(20, 30, 280, 20), "E - Toggle Edit Mode");
            GUI.Label(new Rect(20, 50, 280, 20), "A - Add Control Point");
            GUI.Label(new Rect(20, 70, 280, 20), "Delete - Remove Selected Point");
        }
    }
}