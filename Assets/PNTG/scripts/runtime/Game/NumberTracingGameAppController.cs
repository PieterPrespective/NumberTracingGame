using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace PNTG
{
    /// <summary>
    /// Main game controller that manages number tracing gameplay with multiple number configurations
    /// </summary>
    public class NumberTracingGameAppController : MonoBehaviour
    {
        [SerializeField] private List<NumberConfiguration> numberConfigs = new List<NumberConfiguration>();
        [SerializeField] private bool showControlPoints = false;
        [SerializeField] private bool autoReloadConfig = false;
        [SerializeField] private float reloadInterval = 0.1f; // 100ms
        
        private UIDocument uiDocument;
        private VisualElement rootElement;
        private VisualElement strokeOverlay;
        private VisualElement completePopup;
        private Label scoreLabel;
        private Button quitButton;
        
        private List<StrokeVisualElement> allStrokeVisuals = new List<StrokeVisualElement>();
        private StrokeVisualElement currentStrokeVisual;
        private StrokeData currentStrokeData;
        private NumberConfiguration currentNumberConfig;
        private int currentStrokeIndex = 0;
        private int currentNumberIndex = 0;
        private bool isPressed = false;
        private float lastReloadTime = 0f;
        private int score = 0;
        private Coroutine hidePopupCoroutine;
        
        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument component not found!");
                return;
            }
            
            InitializeUI();
            LoadNextNumber();
        }
        
        private void InitializeUI()
        {
            rootElement = uiDocument.rootVisualElement;
            strokeOverlay = rootElement.Q<VisualElement>("StrokeOverlay");
            
            if (strokeOverlay == null)
            {
                Debug.LogError("StrokeOverlay element not found!");
                return;
            }
            
            // Create score display
            var hudContainer = rootElement.Q<VisualElement>("HUD");
            if (hudContainer == null)
            {
                hudContainer = new VisualElement();
                hudContainer.name = "HUD";
                hudContainer.style.position = Position.Absolute;
                hudContainer.style.top = 10;
                hudContainer.style.right = 10;
                hudContainer.style.flexDirection = FlexDirection.Row;
                hudContainer.style.alignItems = Align.Center;
                rootElement.Add(hudContainer);
            }
            
            // Create score label
            scoreLabel = new Label($"Score: {score}");
            scoreLabel.name = "ScoreLabel";
            scoreLabel.style.fontSize = 32;
            scoreLabel.style.color = Color.white;
            scoreLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            scoreLabel.style.marginRight = 20;
            hudContainer.Add(scoreLabel);
            
            // Create quit button
            quitButton = new Button(() => Application.Quit());
            quitButton.name = "QuitButton";
            quitButton.text = "X";
            quitButton.style.width = 50;
            quitButton.style.height = 50;
            quitButton.style.fontSize = 24;
            quitButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            quitButton.style.color = Color.white;
            quitButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            hudContainer.Add(quitButton);
            
            // Create completion popup (initially hidden)
            completePopup = new VisualElement();
            completePopup.name = "CompletePopup";
            completePopup.style.position = Position.Absolute;
            completePopup.style.left = Length.Percent(50);
            completePopup.style.top = Length.Percent(50);
            completePopup.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
            completePopup.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);
            completePopup.style.paddingTop = 40;
            completePopup.style.paddingBottom = 40;
            completePopup.style.paddingLeft = 60;
            completePopup.style.paddingRight = 60;
            completePopup.style.borderTopLeftRadius = 20;
            completePopup.style.borderTopRightRadius = 20;
            completePopup.style.borderBottomLeftRadius = 20;
            completePopup.style.borderBottomRightRadius = 20;
            completePopup.style.display = DisplayStyle.None;
            
            var popupLabel = new Label("Number Complete!");
            popupLabel.style.fontSize = 48;
            popupLabel.style.color = Color.white;
            popupLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            completePopup.Add(popupLabel);
            
            rootElement.Add(completePopup);
        }
        
        private void LoadNextNumber()
        {
            if (numberConfigs == null || numberConfigs.Count == 0)
            {
                Debug.LogError("No NumberConfigurations assigned! Please assign at least one NumberConfiguration ScriptableObject.");
                return;
            }
            
            // Select next number configuration
            currentNumberIndex = (currentNumberIndex) % numberConfigs.Count;
            currentNumberConfig = numberConfigs[currentNumberIndex];
            currentNumberIndex++;
            
            if (currentNumberConfig == null)
            {
                Debug.LogError($"NumberConfiguration at index {currentNumberIndex - 1} is null!");
                return;
            }
            
            LoadNumberConfiguration();
        }
        
        private void LoadNumberConfiguration()
        {
            if (currentNumberConfig.Strokes == null || currentNumberConfig.Strokes.Length == 0)
            {
                Debug.LogError($"NumberConfiguration '{currentNumberConfig.NumberName}' has no strokes!");
                return;
            }
            
            // Set background image if available
            SetBackgroundImage(currentNumberConfig.BackgroundImage);
            
            // Clear existing strokes
            ClearAllStrokes();
            
            // Create visual elements for all strokes
            for (int i = 0; i < currentNumberConfig.Strokes.Length; i++)
            {
                var strokeData = currentNumberConfig.Strokes[i];
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
            currentStrokeData = currentNumberConfig.Strokes[0];
            
            Debug.Log($"Number {currentNumberConfig.NumberName} loaded with {currentNumberConfig.Strokes.Length} strokes");
            
            // Debug stroke data
            if (currentStrokeData != null && currentStrokeData.ControlPoints != null)
            {
                Debug.Log($"First stroke has {currentStrokeData.ControlPoints.Length} control points, " +
                         $"WobbleRadius: {currentStrokeData.WobbleRadius}");
                for (int i = 0; i < Mathf.Min(3, currentStrokeData.ControlPoints.Length); i++)
                {
                    Debug.Log($"Control Point {i}: {currentStrokeData.ControlPoints[i]}");
                }
            }
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

            // Platform-specific input handling
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            // On mobile platforms, prioritize touch input
            if (touchscreen != null && touchscreen.touches.Count > 0)
            {
                var touch = touchscreen.touches[0];
                inputPosition = touch.position.ReadValue();
                currentlyPressed = touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began || 
                                  touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || 
                                  touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary;
            }
            else if (mouse != null)
            {
                // Fallback to mouse if no touch (for editor testing)
                inputPosition = mouse.position.ReadValue();
                currentlyPressed = mouse.leftButton.isPressed;
            }
            #else
            // On desktop platforms, check mouse first
            if (mouse != null && mouse.leftButton.isPressed)
            {
                inputPosition = mouse.position.ReadValue();
                currentlyPressed = true;
            }
            else if (touchscreen != null && touchscreen.touches.Count > 0)
            {
                // Also support touch on desktop (touchscreen monitors)
                var touch = touchscreen.touches[0];
                inputPosition = touch.position.ReadValue();
                currentlyPressed = touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began || 
                                  touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || 
                                  touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary;
            }
            #endif
            
            // Convert screen position to UI position
            // Method 1: Try using RuntimePanelUtils if available
            Vector2 uiPosition;
            Vector2 panelPosition = Vector2.zero;
            if (strokeOverlay.panel != null)
            {
                // Screen coordinates need Y-axis flipped for UI coordinates
                var flippedScreenPos = new Vector2(inputPosition.x, Screen.height - inputPosition.y);
                panelPosition = RuntimePanelUtils.ScreenToPanel(strokeOverlay.panel, flippedScreenPos);
                uiPosition = strokeOverlay.WorldToLocal(panelPosition);
            }
            else
            {
                // Method 2: Fallback to manual conversion with Y-axis flip
                var rect = strokeOverlay.worldBound;
                uiPosition = new Vector2(
                    (inputPosition.x - rect.x) * strokeOverlay.resolvedStyle.width / rect.width,
                    (Screen.height - inputPosition.y - rect.y) * strokeOverlay.resolvedStyle.height / rect.height
                );
            }
            
            // Debug logging
            if (currentlyPressed && Time.frameCount % 30 == 0) // Log every 30 frames
            {
                var rect = strokeOverlay.worldBound;
                Debug.Log($"Input Debug - Screen: {inputPosition}, Panel: {panelPosition}, UI: {uiPosition}, " +
                         $"WorldBound: {rect}, ResolvedStyle: {strokeOverlay.resolvedStyle.width}x{strokeOverlay.resolvedStyle.height}, " +
                         $"Screen Size: {Screen.width}x{Screen.height}");
            }
            
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
                
                Debug.Log($"Result: {result.isValid}, Progress: {result.progress:F3}, Visual: {result.visualProgress:F3}, Distance: {result.distanceFromStroke:F1}, isPressed: {isPressed}");

                if (result.isValid)
                {
                    currentStrokeVisual.Progress = result.visualProgress;
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
                currentStrokeData = currentNumberConfig.Strokes[currentStrokeIndex];
                currentStrokeVisual.ShowWobbleRadius = true;
                currentStrokeVisual.Progress = 0f;
                
                Debug.Log($"Starting stroke {currentStrokeIndex + 1} of {allStrokeVisuals.Count}");
            }
            else
            {
                // All strokes completed - number is complete!
                OnNumberCompleted();
            }
        }
        
        private void OnNumberCompleted()
        {
            Debug.Log($"Number {currentNumberConfig.NumberName} completed!");
            
            // Increment score
            score++;
            UpdateScoreDisplay();
            
            // Show completion popup
            ShowCompletionPopup();
            
            // Clear current stroke references
            currentStrokeVisual = null;
            currentStrokeData = null;
            
            // Load next number after delay
            StartCoroutine(LoadNextNumberAfterDelay(3f));
        }
        
        private void UpdateScoreDisplay()
        {
            if (scoreLabel != null)
            {
                scoreLabel.text = $"Score: {score}";
            }
        }
        
        private void ShowCompletionPopup()
        {
            if (completePopup != null)
            {
                completePopup.style.display = DisplayStyle.Flex;
                
                // Cancel any existing hide coroutine
                if (hidePopupCoroutine != null)
                {
                    StopCoroutine(hidePopupCoroutine);
                }
                
                // Start new hide coroutine
                hidePopupCoroutine = StartCoroutine(HidePopupAfterDelay(3f));
            }
        }
        
        private IEnumerator HidePopupAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (completePopup != null)
            {
                completePopup.style.display = DisplayStyle.None;
            }
        }
        
        private IEnumerator LoadNextNumberAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            LoadNextNumber();
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
            if (currentNumberConfig == null)
                return;
                
            // Check if the configuration has been modified
            if (currentNumberConfig.Strokes != null && currentNumberConfig.Strokes.Length > 0)
            {
                // Update all stroke visuals with new data
                for (int i = 0; i < allStrokeVisuals.Count && i < currentNumberConfig.Strokes.Length; i++)
                {
                    allStrokeVisuals[i].StrokeData = currentNumberConfig.Strokes[i];
                }
                
                // Update current stroke data if still active
                if (currentStrokeData != null && currentStrokeIndex < currentNumberConfig.Strokes.Length)
                {
                    currentStrokeData = currentNumberConfig.Strokes[currentStrokeIndex];
                }
                
                // Update background image if changed
                SetBackgroundImage(currentNumberConfig.BackgroundImage);
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
            var backgroundElement = rootElement.Q<VisualElement>("Number2Background");
            
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
        
        private void OnGUI()
        {
            // Draw control panel
            GUI.Box(new Rect(10, 10, 320, 160), "Number Tracing Game Controls");
            
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
            if (currentNumberConfig != null)
            {
                GUI.Box(new Rect(10, 180, 320, 100), "Current Number Info");
                GUI.Label(new Rect(20, 205, 300, 20), $"Number: {currentNumberConfig.NumberName}");
                GUI.Label(new Rect(20, 225, 300, 20), $"Strokes: {(currentNumberConfig.Strokes?.Length ?? 0)}");
                GUI.Label(new Rect(20, 245, 300, 20), $"Background: {(currentNumberConfig.BackgroundImage?.name ?? "None")}");
                GUI.Label(new Rect(20, 265, 300, 20), $"Numbers in Game: {numberConfigs.Count}");
            }
        }
    }
}