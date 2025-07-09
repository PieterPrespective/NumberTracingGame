using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PNTG.Editor
{
    /// <summary>
    /// Editor utility to create the demo scene with number 2
    /// </summary>
    public static class DemoSceneCreator
    {
        [MenuItem("PNTG/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create UI Document GameObject
            var uiDocumentGO = new GameObject("UIDocument");
            var uiDocument = uiDocumentGO.AddComponent<UIDocument>();
            
            // Load the Number2Demo UXML
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/PNTG/UIElements/Number2Demo.uxml");
            if (uxml != null)
            {
                uiDocument.visualTreeAsset = uxml;
            }
            
            // Create Number 2 configuration
            CreateNumber2Configuration();
            
            // Add demo controller
            var demoController = uiDocumentGO.AddComponent<Number2DemoController>();
            
            // Save scene
            string scenePath = "Assets/PNTG/scenes/Number2Demo.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"Demo scene created at: {scenePath}");
        }
        
        private static void CreateNumber2Configuration()
        {
            // Create Number2 ScriptableObject if it doesn't exist
            string configPath = "Assets/PNTG/ScriptableObjects/Number2Config.asset";
            
            var config = AssetDatabase.LoadAssetAtPath<NumberConfiguration>(configPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<NumberConfiguration>();
                
                // Set basic properties
                var serializedObject = new SerializedObject(config);
                serializedObject.FindProperty("numberName").stringValue = "2";
                serializedObject.FindProperty("scoreValue").intValue = 1;
                serializedObject.ApplyModifiedProperties();
                
                // Create directory if needed
                if (!AssetDatabase.IsValidFolder("Assets/PNTG/ScriptableObjects"))
                {
                    AssetDatabase.CreateFolder("Assets/PNTG", "ScriptableObjects");
                }
                
                AssetDatabase.CreateAsset(config, configPath);
                
                // Add stroke with control points for number 2
                config.AddStroke();
                var stroke = config.Strokes[0];
                
                // Define control points for number 2 curve
                var controlPoints = new NURBSControlPoint[]
                {
                    new NURBSControlPoint(new Vector2(150, 100), 1f),    // Start - top left
                    new NURBSControlPoint(new Vector2(350, 100), 1f),    // Top right
                    new NURBSControlPoint(new Vector2(400, 200), 1f),    // Right curve
                    new NURBSControlPoint(new Vector2(350, 300), 1f),    // Mid right
                    new NURBSControlPoint(new Vector2(200, 400), 1f),    // Center diagonal
                    new NURBSControlPoint(new Vector2(100, 500), 1f),    // Bottom left
                    new NURBSControlPoint(new Vector2(100, 600), 1f),    // Bottom horizontal start
                    new NURBSControlPoint(new Vector2(400, 600), 1f)     // Bottom horizontal end
                };
                
                stroke.SetControlPoints(controlPoints);
                
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"Number 2 configuration created at: {configPath}");
            }
        }
    }
}