using UnityEngine;
using UnityEditor;

namespace PNTG.Editor
{
    /// <summary>
    /// Editor utility to create NumberConfiguration ScriptableObjects for numbers 0-3
    /// Based on the reference images from Claude_Prompts/Numbers
    /// </summary>
    public static class CreateNumberConfigurations
    {
        [MenuItem("PNTG/Create All Number Configurations")]
        public static void CreateAllNumberConfigurations()
        {
            CreateNumber0Config();
            CreateNumber1Config();
            CreateNumber2Config();
            CreateNumber3Config();
            
            AssetDatabase.SaveAssets();
            Debug.Log("All number configurations created successfully!");
        }
        
        [MenuItem("PNTG/Create Number 0 Config")]
        public static void CreateNumber0Config()
        {
            var config = CreateNumberConfiguration("0", "Trace_0_Instruction.JPG");
            
            // Number 0 - Single stroke, oval shape (clockwise from top)
            config.AddStroke();
            var stroke = config.Strokes[0];
            
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(300, 100), 1f),    // Top center
                new NURBSControlPoint(new Vector2(450, 150), 1f),    // Top right
                new NURBSControlPoint(new Vector2(500, 300), 1f),    // Right middle
                new NURBSControlPoint(new Vector2(450, 450), 1f),    // Bottom right
                new NURBSControlPoint(new Vector2(300, 500), 1f),    // Bottom center
                new NURBSControlPoint(new Vector2(150, 450), 1f),    // Bottom left
                new NURBSControlPoint(new Vector2(100, 300), 1f),    // Left middle
                new NURBSControlPoint(new Vector2(150, 150), 1f),    // Top left
                new NURBSControlPoint(new Vector2(300, 100), 1f)     // Close the oval
            };
            
            stroke.SetControlPoints(controlPoints);
            
            SaveConfiguration(config, "Number0Config");
        }
        
        [MenuItem("PNTG/Create Number 1 Config")]
        public static void CreateNumber1Config()
        {
            var config = CreateNumberConfiguration("1", "Trace_1_Instruction.JPG");
            
            // Number 1 - Single stroke, straight line from top to bottom
            config.AddStroke();
            var stroke = config.Strokes[0];
            
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(300, 100), 1f),    // Top
                new NURBSControlPoint(new Vector2(300, 300), 1f),    // Middle
                new NURBSControlPoint(new Vector2(300, 500), 1f)     // Bottom
            };
            
            stroke.SetControlPoints(controlPoints);
            
            SaveConfiguration(config, "Number1Config");
        }
        
        [MenuItem("PNTG/Create Number 2 Config")]
        public static void CreateNumber2Config()
        {
            var config = CreateNumberConfiguration("2", "Trace_2_Instruction.JPG");
            
            // Number 2 - Single stroke, curve then diagonal then horizontal
            config.AddStroke();
            var stroke = config.Strokes[0];
            
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(150, 150), 1f),    // Start - top left curve
                new NURBSControlPoint(new Vector2(300, 100), 1f),    // Top curve middle
                new NURBSControlPoint(new Vector2(450, 150), 1f),    // Top right
                new NURBSControlPoint(new Vector2(500, 200), 1f),    // Right curve start
                new NURBSControlPoint(new Vector2(450, 250), 1f),    // Right curve end
                new NURBSControlPoint(new Vector2(350, 300), 1f),    // Diagonal start
                new NURBSControlPoint(new Vector2(250, 400), 1f),    // Diagonal middle
                new NURBSControlPoint(new Vector2(150, 450), 1f),    // Bottom left
                new NURBSControlPoint(new Vector2(150, 500), 1f),    // Bottom horizontal start
                new NURBSControlPoint(new Vector2(300, 500), 1f),    // Bottom horizontal middle
                new NURBSControlPoint(new Vector2(450, 500), 1f)     // Bottom horizontal end
            };
            
            stroke.SetControlPoints(controlPoints);
            
            SaveConfiguration(config, "Number2Config");
        }
        
        [MenuItem("PNTG/Create Number 3 Config")]
        public static void CreateNumber3Config()
        {
            var config = CreateNumberConfiguration("3", "Trace_2_Instruction.JPG"); // Note: Assignment says to use Trace_2 for number 3
            
            // Number 3 - Single stroke, two curves stacked
            config.AddStroke();
            var stroke = config.Strokes[0];
            
            var controlPoints = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(150, 150), 1f),    // Top left
                new NURBSControlPoint(new Vector2(300, 100), 1f),    // Top curve start
                new NURBSControlPoint(new Vector2(450, 150), 1f),    // Top right
                new NURBSControlPoint(new Vector2(400, 200), 1f),    // Top curve end
                new NURBSControlPoint(new Vector2(350, 250), 1f),    // Middle connection
                new NURBSControlPoint(new Vector2(300, 300), 1f),    // Middle point
                new NURBSControlPoint(new Vector2(400, 350), 1f),    // Bottom curve start
                new NURBSControlPoint(new Vector2(450, 400), 1f),    // Bottom right
                new NURBSControlPoint(new Vector2(350, 450), 1f),    // Bottom curve middle
                new NURBSControlPoint(new Vector2(200, 500), 1f),    // Bottom left
                new NURBSControlPoint(new Vector2(150, 450), 1f)     // Bottom curve end
            };
            
            stroke.SetControlPoints(controlPoints);
            
            SaveConfiguration(config, "Number3Config");
        }
        
        private static NumberConfiguration CreateNumberConfiguration(string numberName, string imageName)
        {
            var config = ScriptableObject.CreateInstance<NumberConfiguration>();
            
            // Set basic properties
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("numberName").stringValue = numberName;
            serializedObject.FindProperty("scoreValue").intValue = 1;
            
            // Load and assign background image
            string imagePath = $"Assets/PNTG/textures/Numbers/{imageName}";
            var backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            
            if (backgroundSprite != null)
            {
                serializedObject.FindProperty("backgroundImage").objectReferenceValue = backgroundSprite;
                Debug.Log($"Background image loaded for number {numberName}: {imagePath}");
            }
            else
            {
                Debug.LogWarning($"Background image not found for number {numberName}: {imagePath}");
            }
            
            serializedObject.ApplyModifiedProperties();
            
            return config;
        }
        
        private static void SaveConfiguration(NumberConfiguration config, string fileName)
        {
            // Create directory if needed
            if (!AssetDatabase.IsValidFolder("Assets/PNTG/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets/PNTG", "ScriptableObjects");
            }
            
            // Save the asset
            string path = $"Assets/PNTG/ScriptableObjects/{fileName}.asset";
            AssetDatabase.CreateAsset(config, path);
            
            EditorUtility.SetDirty(config);
            
            Debug.Log($"Number {config.NumberName} configuration created at: {path}");
            Debug.Log($"Wobble radius: {config.Strokes[0].WobbleRadius}px");
        }
    }
}