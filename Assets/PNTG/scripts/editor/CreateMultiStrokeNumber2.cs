using UnityEngine;
using UnityEditor;

namespace PNTG.Editor
{
    /// <summary>
    /// Editor utility to create a multi-stroke Number 2 configuration
    /// </summary>
    public static class CreateMultiStrokeNumber2
    {
        [MenuItem("PNTG/Create Multi-Stroke Number 2 Config")]
        public static void CreateMultiStrokeNumber2Config()
        {
            var config = ScriptableObject.CreateInstance<NumberConfiguration>();
            
            // Set basic properties
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("numberName").stringValue = "2";
            serializedObject.FindProperty("scoreValue").intValue = 1;
            
            // Load background image
            string imagePath = "Assets/PNTG/textures/Numbers/Trace_2_Instruction.JPG";
            var backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            
            if (backgroundSprite != null)
            {
                serializedObject.FindProperty("backgroundImage").objectReferenceValue = backgroundSprite;
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Add first stroke - curved top part
            config.AddStroke();
            var stroke1 = config.Strokes[0];
            
            var controlPoints1 = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(150, 150), 1f),    // Start - top left
                new NURBSControlPoint(new Vector2(250, 100), 1f),    // Top curve control
                new NURBSControlPoint(new Vector2(350, 120), 1f),    // Top right
                new NURBSControlPoint(new Vector2(400, 180), 1f),    // Right curve start
                new NURBSControlPoint(new Vector2(380, 240), 1f),    // Right curve middle
                new NURBSControlPoint(new Vector2(320, 280), 1f)     // End at middle connection
            };
            
            stroke1.SetControlPoints(controlPoints1);
            
            // Add second stroke - diagonal and bottom line
            config.AddStroke();
            var stroke2 = config.Strokes[1];
            
            var controlPoints2 = new NURBSControlPoint[]
            {
                new NURBSControlPoint(new Vector2(320, 280), 1f),    // Start from middle connection
                new NURBSControlPoint(new Vector2(280, 350), 1f),    // Diagonal middle
                new NURBSControlPoint(new Vector2(200, 420), 1f),    // Diagonal end
                new NURBSControlPoint(new Vector2(150, 480), 1f),    // Bottom left
                new NURBSControlPoint(new Vector2(150, 500), 1f),    // Bottom horizontal start
                new NURBSControlPoint(new Vector2(280, 500), 1f),    // Bottom horizontal middle
                new NURBSControlPoint(new Vector2(420, 500), 1f)     // Bottom horizontal end
            };
            
            stroke2.SetControlPoints(controlPoints2);
            
            // Create directory if needed
            if (!AssetDatabase.IsValidFolder("Assets/PNTG/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets/PNTG", "ScriptableObjects");
            }
            
            // Save the asset
            string path = "Assets/PNTG/ScriptableObjects/MultiStrokeNumber2Config.asset";
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            
            EditorUtility.SetDirty(config);
            
            Debug.Log($"Multi-stroke Number 2 configuration created at: {path}");
            Debug.Log($"Stroke 1 wobble radius: {stroke1.WobbleRadius}px");
            Debug.Log($"Stroke 2 wobble radius: {stroke2.WobbleRadius}px");
            
            // Select the created asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}