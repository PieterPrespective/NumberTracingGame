using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace PNTG
{
    /// <summary>
    /// Demo controller specifically for showing number 2 with a single stroke
    /// </summary>
    public class Number2DemoController : MonoBehaviour
    {
        [SerializeField] private NumberConfiguration numberConfig;
        [SerializeField] private bool showControlPoints = false;
        [SerializeField] private bool autoReloadConfig = false;
        [SerializeField] private float reloadInterval = 0.1f; // 100ms
        
        private UIDocument uiDocument;
        private VisualElement strokeOverlay;
        private Label mainTitle;
        private Label numberDisplay;
        private List<StrokeVisualElement> allStrokeVisuals = new List<StrokeVisualElement>();
        private StrokeVisualElement currentStrokeVisual;
        private StrokeData currentStrokeData;
        private int currentStrokeIndex = 0;
        private bool isPressed = false;
        private float lastReloadTime = 0f;
        
        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument component not found!");
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            strokeOverlay = root.Q<VisualElement>("StrokeOverlay");
            mainTitle = root.Q<Label>("MainTitle");
            numberDisplay = root.Q<Label>("NumberDisplay");
            
            if (strokeOverlay == null)
            {
                Debug.LogError("StrokeOverlay element not found!");
                return;
            }
            
            if (mainTitle == null)
            {
                Debug.LogError("MainTitle element not found in UXML!");
                return;
            }
            else
            {
                Debug.Log("MainTitle element found successfully");
            }
            
            if (numberDisplay == null)
            {
                Debug.LogError("NumberDisplay element not found in UXML!");
                return;
            }
            else
            {
                Debug.Log("NumberDisplay element found successfully");
            }
            
            LoadNumberConfiguration();
        }
        
        private void LoadNumberConfiguration()
        {
            if (numberConfig == null)
            {
                Debug.LogError("NumberConfiguration not assigned! Please assign a NumberConfiguration ScriptableObject.");
                CreateFallbackStroke();
                return;
            }
            
            Debug.Log($"Loading NumberConfiguration: {numberConfig.name}, NumberName: '{numberConfig.NumberName}'");
            
            if (numberConfig.Strokes == null || numberConfig.Strokes.Length == 0)
            {
                Debug.LogError("NumberConfiguration has no strokes! Please configure at least one stroke.");
                CreateFallbackStroke();
                return;
            }
            
            // Update UI text based on NumberConfiguration
            UpdateUIText();
            
            // Set background image if available
            SetBackgroundImage(numberConfig.BackgroundImage);
            
            // Clear existing strokes
            ClearAllStrokes();
            
            // Create visual elements for all strokes
            for (int i = 0; i < numberConfig.Strokes.Length; i++)
            {
                var strokeData = numberConfig.Strokes[i];
                var strokeVisual = new StrokeVisualElement
                {
                    StrokeData = strokeData,
                    ShowWobbleRadius = (i == 0), // Only show wobble for first stroke
                    ShowControlPoints = showControlPoints,
                    IsCompleted = false,
                    Progress = 0f
                };
                
                strokeOverlay.Add(strokeVisual);
                allStrokeVisuals.Add(strokeVisual);
            }
            
            // Set current stroke to the first one
            currentStrokeIndex = 0;
            currentStrokeVisual = allStrokeVisuals[0];
            currentStrokeData = numberConfig.Strokes[0];
            
            Debug.Log($"Number {numberConfig.NumberName} loaded with {numberConfig.Strokes.Length} strokes");
            Debug.Log($"Current stroke wobble radius: {currentStrokeData.WobbleRadius}px");
            Debug.Log($"Control points visibility: {showControlPoints}, Auto-reload: {autoReloadConfig}");
        }
        
        private void CreateFallbackStroke()
        {
            // Fallback stroke data if no configuration is provided
            var strokeData = new StrokeData();
            
            // Define control points that create the shape of number 2
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(150, 100), 1f),    // Start - top left curve
                new NURBSControlPoint(new Vector2(250, 80), 1f),     // Top curve control
                new NURBSControlPoint(new Vector2(350, 100), 1f),    // Top right
                new NURBSControlPoint(new Vector2(380, 150), 1f),    // Right curve start
                new NURBSControlPoint(new Vector2(380, 250), 1f),    // Right curve middle
                new NURBSControlPoint(new Vector2(300, 350), 1f),    // Diagonal start
                new NURBSControlPoint(new Vector2(200, 450), 1f),    // Diagonal middle
                new NURBSControlPoint(new Vector2(100, 550), 1f),    // Bottom left
                new NURBSControlPoint(new Vector2(100, 600), 1f),    // Bottom curve start
                new NURBSControlPoint(new Vector2(250, 600), 1f),    // Bottom middle
                new NURBSControlPoint(new Vector2(400, 600), 1f)     // Bottom right end
            };
            
            strokeData.SetControlPoints(controlPoints);
            
            // Clear existing strokes
            ClearAllStrokes();
            
            // Create visual element
            var strokeVisual = new StrokeVisualElement
            {
                StrokeData = strokeData,
                ShowWobbleRadius = true,
                ShowControlPoints = showControlPoints,
                Progress = 0f
            };
            
            strokeOverlay.Add(strokeVisual);
            allStrokeVisuals.Add(strokeVisual);
            
            currentStrokeIndex = 0;
            currentStrokeVisual = strokeVisual;
            currentStrokeData = strokeData;
            
            Debug.Log("Fallback Number 2 stroke created with default 20 pixel wobble radius");
        }
        
        private void ClearAllStrokes()
        {
            strokeOverlay.Clear();
            allStrokeVisuals.Clear();
            currentStrokeVisual = null;
            currentStrokeData = null;
            StrokeInputValidator.ClearAllProgress();
        }
        
        private void Update()
        {
            if (currentStrokeVisual == null)
                return;
                
            // Handle toggles
            HandleToggles();
            
            // Auto-reload configuration if enabled
            if (autoReloadConfig && Time.time - lastReloadTime > reloadInterval)
            {
                ReloadConfiguration();
                lastReloadTime = Time.time;
            }
            
            // Update trace fading for current stroke
            currentStrokeVisual.UpdateTrace();
            
            // Handle input only if stroke data is valid
            if (currentStrokeData != null)
            {
                HandleInput();
            }
        }
        
        private void HandleInput()
        {
            var mouse = Mouse.current;
            var touchscreen = Touchscreen.current;
            
            Vector2 inputPosition = Vector2.zero;
            bool currentlyPressed = false;
            
            if (mouse != null)
            {
                inputPosition = mouse.position.ReadValue();
                currentlyPressed = mouse.leftButton.isPressed;
            }
            else if (touchscreen != null && touchscreen.touches.Count > 0)
            {
                var touch = touchscreen.touches[0];
                inputPosition = touch.position.ReadValue();
                currentlyPressed = touch.isInProgress;
            }
            
            // Convert screen position to UI position
            var rect = strokeOverlay.worldBound;
            var uiPosition = new Vector2(
                (inputPosition.x - rect.x) / rect.width * strokeOverlay.resolvedStyle.width,
                (Screen.height - inputPosition.y - rect.y) / rect.height * strokeOverlay.resolvedStyle.height
            );
            
            // Handle input validation and tracing
            if (currentlyPressed)
            {
                var inputParams = new StrokeInputParams
                {
                    position = uiPosition,
                    isPressed = true,
                    deltaTime = Time.deltaTime
                };
                
                var result = StrokeInputValidator.ValidateInput(currentStrokeData, inputParams);
                
                if (result.isValid)
                {
                    currentStrokeVisual.Progress = result.progress;
                    currentStrokeVisual.AddTracePoint(uiPosition);
                    
                    // Check if stroke is completed
                    if (result.isComplete)
                    {
                        OnStrokeCompleted();
                    }
                }
                else if (isPressed)
                {
                    // Reset if moved outside valid area
                    StrokeInputValidator.ResetProgress(currentStrokeData);
                    currentStrokeVisual.Progress = 0f;
                    currentStrokeVisual.ClearTrace();
                }
            }
            else if (isPressed)
            {
                // Released - reset if not complete
                StrokeInputValidator.ResetProgress(currentStrokeData);
                currentStrokeVisual.Progress = 0f;
            }
            
            isPressed = currentlyPressed;
        }
        
        private void OnStrokeCompleted()
        {
            // Mark current stroke as completed
            currentStrokeVisual.IsCompleted = true;
            currentStrokeVisual.Progress = 1f;
            currentStrokeVisual.ShowWobbleRadius = false;
            
            Debug.Log($"Stroke {currentStrokeIndex + 1} completed!");
            
            // Move to next stroke if available
            currentStrokeIndex++;
            if (currentStrokeIndex < allStrokeVisuals.Count)
            {
                // Set up next stroke
                currentStrokeVisual = allStrokeVisuals[currentStrokeIndex];
                currentStrokeData = numberConfig.Strokes[currentStrokeIndex];
                currentStrokeVisual.ShowWobbleRadius = true;
                currentStrokeVisual.Progress = 0f;
                
                Debug.Log($"Starting stroke {currentStrokeIndex + 1} of {allStrokeVisuals.Count}");
            }
            else
            {
                // All strokes completed
                Debug.Log($"Number {numberConfig.NumberName} completed! All {allStrokeVisuals.Count} strokes finished.");
                currentStrokeVisual = null;
                currentStrokeData = null;
            }
        }
        
        private void HandleToggles()
        {
            // Handle control points toggle
            if (Keyboard.current[Key.C].wasPressedThisFrame)
            {
                showControlPoints = !showControlPoints;
                foreach (var strokeVis in allStrokeVisuals)
                {
                    strokeVis.ShowControlPoints = showControlPoints;
                }
                Debug.Log($"Control points: {(showControlPoints ? "ON" : "OFF")}");
            }
            
            // Handle auto-reload toggle
            if (Keyboard.current[Key.R].wasPressedThisFrame)
            {
                autoReloadConfig = !autoReloadConfig;
                Debug.Log($"Auto-reload config: {(autoReloadConfig ? "ON" : "OFF")}");
                if (autoReloadConfig)
                {
                    lastReloadTime = Time.time;
                }
            }
        }
        
        private void ReloadConfiguration()
        {
            if (numberConfig == null)
                return;
                
            // Check if the configuration has been modified
            if (numberConfig.Strokes != null && numberConfig.Strokes.Length > 0)
            {
                // Update all stroke visuals with new data
                for (int i = 0; i < allStrokeVisuals.Count && i < numberConfig.Strokes.Length; i++)
                {
                    allStrokeVisuals[i].StrokeData = numberConfig.Strokes[i];
                }
                
                // Update current stroke data if still active
                if (currentStrokeData != null && currentStrokeIndex < numberConfig.Strokes.Length)
                {
                    currentStrokeData = numberConfig.Strokes[currentStrokeIndex];
                }
                
                // Update background image if changed
                SetBackgroundImage(numberConfig.BackgroundImage);
                
                // Update UI text
                UpdateUIText();
            }
        }
        
        private void SetBackgroundImage(Sprite backgroundSprite)
        {
            if (backgroundSprite == null)
            {
                Debug.LogWarning("No background image provided");
                return;
            }
            
            // Find the background element
            var root = uiDocument.rootVisualElement;
            var backgroundElement = root.Q<VisualElement>("Number2Background");
            
            if (backgroundElement == null)
            {
                Debug.LogWarning("Background element not found in UI");
                return;
            }
            
            // Set the background image
            backgroundElement.style.backgroundImage = new StyleBackground(backgroundSprite);
            backgroundElement.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            
            Debug.Log($"Background image set: {backgroundSprite.name}");
        }
        
        private void UpdateUIText()
        {
            if (numberConfig == null || mainTitle == null || numberDisplay == null)
            {
                Debug.LogWarning($"UpdateUIText failed - numberConfig: {numberConfig != null}, mainTitle: {mainTitle != null}, numberDisplay: {numberDisplay != null}");
                return;
            }
            
            // Get the number name, use fallback if null or empty
            string numberName = string.IsNullOrEmpty(numberConfig.NumberName) ? "2" : numberConfig.NumberName;
            Debug.Log($"NumberName from config: '{numberConfig.NumberName}', using: '{numberName}'");
                
            // Update main title with Dutch text and number name
            string titleText = $"Teken het onderstaande nummer {numberName}";
            mainTitle.text = titleText;
            Debug.Log($"Set mainTitle.text to: '{titleText}', actual text now: '{mainTitle.text}'");
            
            // Update the large number display in the center
            numberDisplay.text = numberName;
            Debug.Log($"Set numberDisplay.text to: '{numberName}', actual text now: '{numberDisplay.text}'");
        }
        
        private void OnGUI()
        {
            // Draw control panel
            GUI.Box(new Rect(10, 10, 320, 140), "Number 2 Demo Controls");
            
            GUI.Label(new Rect(20, 35, 300, 20), "C - Toggle Control Points");
            GUI.Label(new Rect(20, 55, 300, 20), "R - Toggle Auto-Reload Config");
            GUI.Label(new Rect(20, 75, 300, 20), "Mouse - Trace the number");
            
            GUI.Label(new Rect(20, 100, 150, 20), $"Control Points: {(showControlPoints ? "ON" : "OFF")}");
            GUI.Label(new Rect(180, 100, 130, 20), $"Auto-Reload: {(autoReloadConfig ? "ON" : "OFF")}");
            
            if (currentStrokeData != null)
            {
                GUI.Label(new Rect(20, 120, 300, 20), $"Wobble Radius: {currentStrokeData.WobbleRadius:F1}px");
            }
            
            // Show stroke progression info
            if (allStrokeVisuals.Count > 0)
            {
                int completedStrokes = 0;
                for (int i = 0; i < allStrokeVisuals.Count; i++)
                {
                    if (allStrokeVisuals[i].IsCompleted) completedStrokes++;
                }
                GUI.Label(new Rect(20, 140, 300, 20), $"Stroke Progress: {completedStrokes}/{allStrokeVisuals.Count}");
            }
            
            // Show config info
            if (numberConfig != null)
            {
                GUI.Box(new Rect(10, 160, 320, 80), "Configuration Info");
                GUI.Label(new Rect(20, 185, 300, 20), $"Number: {numberConfig.NumberName}");
                GUI.Label(new Rect(20, 205, 300, 20), $"Strokes: {(numberConfig.Strokes?.Length ?? 0)}");
                GUI.Label(new Rect(20, 220, 300, 20), $"Background: {(numberConfig.BackgroundImage?.name ?? "None")}");
            }
        }
    }
}