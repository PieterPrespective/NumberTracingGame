using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace PNTG
{
    /// <summary>
    /// Main gameplay controller responsible for game loop and state management
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private NumberConfiguration[] numberConfigurations;
        [SerializeField] private float strokeCompletionDelay = 0.5f;
        
        private VisualElement root;
        private Label scoreLabel;
        private Button quitButton;
        private VisualElement numberBackground;
        private VisualElement strokeOverlay;
        
        private NumberConfiguration currentNumber;
        private int currentStrokeIndex = 0;
        private int score = 0;
        private StrokeVisualElement currentStrokeVisual;
        private List<StrokeVisualElement> completedStrokes = new List<StrokeVisualElement>();
        
        private bool isProcessingInput = false;
        private Vector2 lastInputPosition;
        private bool isInputPressed = false;
        
        private void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
        }
        
        private void Start()
        {
            InitializeUI();
            LoadNextNumber();
        }
        
        private void InitializeUI()
        {
            root = uiDocument.rootVisualElement;
            
            scoreLabel = root.Q<Label>("ScoreValue");
            quitButton = root.Q<Button>("QuitButton");
            numberBackground = root.Q<VisualElement>("NumberBackground");
            strokeOverlay = root.Q<VisualElement>("StrokeOverlay");
            
            quitButton.clicked += OnQuitClicked;
            
            UpdateScore();
        }
        
        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        private void LoadNextNumber()
        {
            if (numberConfigurations == null || numberConfigurations.Length == 0)
            {
                Debug.LogError("No number configurations available!");
                return;
            }
            
            // Clear previous strokes
            ClearStrokes();
            
            // Select random number
            currentNumber = numberConfigurations[Random.Range(0, numberConfigurations.Length)];
            currentStrokeIndex = 0;
            
            // Set background image
            if (currentNumber.BackgroundImage != null)
            {
                numberBackground.style.backgroundImage = new StyleBackground(currentNumber.BackgroundImage);
            }
            
            // Load first stroke
            LoadCurrentStroke();
        }
        
        private void LoadCurrentStroke()
        {
            if (currentNumber == null || currentStrokeIndex >= currentNumber.Strokes.Length)
                return;
                
            var strokeData = currentNumber.Strokes[currentStrokeIndex];
            
            // Create new stroke visual
            currentStrokeVisual = new StrokeVisualElement
            {
                StrokeData = strokeData,
                ShowWobbleRadius = true,
                Progress = 0f
            };
            
            strokeOverlay.Add(currentStrokeVisual);
            isProcessingInput = true;
        }
        
        private void ClearStrokes()
        {
            strokeOverlay.Clear();
            completedStrokes.Clear();
            currentStrokeVisual = null;
            StrokeInputValidator.ClearAllProgress();
        }
        
        private void Update()
        {
            if (!isProcessingInput || currentStrokeVisual == null)
                return;
                
            // Handle input
            HandleInput();
        }
        
        private void HandleInput()
        {
            // Get input from mouse or touch
            var mouse = Mouse.current;
            var touchscreen = Touchscreen.current;
            
            if (mouse != null)
            {
                lastInputPosition = mouse.position.ReadValue();
                isInputPressed = mouse.leftButton.isPressed;
            }
            else if (touchscreen != null && touchscreen.touches.Count > 0)
            {
                var touch = touchscreen.touches[0];
                lastInputPosition = touch.position.ReadValue();
                isInputPressed = touch.isInProgress;
            }
            else
            {
                isInputPressed = false;
            }
            
            // Convert screen position to UI position
            var rect = strokeOverlay.worldBound;
            var uiPosition = new Vector2(
                (lastInputPosition.x - rect.x) / rect.width * strokeOverlay.resolvedStyle.width,
                (Screen.height - lastInputPosition.y - rect.y) / rect.height * strokeOverlay.resolvedStyle.height
            );
            
            // Validate input
            var inputParams = new StrokeInputParams
            {
                position = uiPosition,
                isPressed = isInputPressed,
                deltaTime = Time.deltaTime
            };
            
            var result = StrokeInputValidator.ValidateInput(currentStrokeVisual.StrokeData, inputParams);
            
            // Update visual progress
            if (result.isValid)
            {
                currentStrokeVisual.Progress = result.progress;
            }
            
            // Check if stroke completed
            if (result.isComplete)
            {
                OnStrokeCompleted();
            }
        }
        
        private void OnStrokeCompleted()
        {
            isProcessingInput = false;
            
            // Mark stroke as completed
            currentStrokeVisual.Progress = 1f;
            currentStrokeVisual.ShowWobbleRadius = false;
            completedStrokes.Add(currentStrokeVisual);
            
            currentStrokeIndex++;
            
            // Check if all strokes completed
            if (currentStrokeIndex >= currentNumber.Strokes.Length)
            {
                OnNumberCompleted();
            }
            else
            {
                // Load next stroke after delay
                StartCoroutine(LoadNextStrokeAfterDelay());
            }
        }
        
        private IEnumerator LoadNextStrokeAfterDelay()
        {
            yield return new WaitForSeconds(strokeCompletionDelay);
            LoadCurrentStroke();
        }
        
        private void OnNumberCompleted()
        {
            // Add score
            score += currentNumber.ScoreValue;
            UpdateScore();
            
            // Load next number after delay
            StartCoroutine(LoadNextNumberAfterDelay());
        }
        
        private IEnumerator LoadNextNumberAfterDelay()
        {
            yield return new WaitForSeconds(strokeCompletionDelay * 2);
            LoadNextNumber();
        }
        
        private void UpdateScore()
        {
            if (scoreLabel != null)
            {
                scoreLabel.text = score.ToString();
            }
        }
    }
}