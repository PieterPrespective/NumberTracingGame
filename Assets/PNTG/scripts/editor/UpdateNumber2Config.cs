using UnityEngine;
using UnityEditor;

namespace PNTG.Editor
{
    /// <summary>
    /// Editor utility to update existing Number2Config with background image
    /// </summary>
    public static class UpdateNumber2Config
    {
        [MenuItem("PNTG/Update Number 2 Config with Background")]
        public static void UpdateNumber2ConfigWithBackground()
        {
            // Load existing config
            string configPath = "Assets/PNTG/ScriptableObjects/Number2Config.asset";
            var config = AssetDatabase.LoadAssetAtPath<NumberConfiguration>(configPath);
            
            if (config == null)
            {
                Debug.LogError($"Number2Config not found at {configPath}");
                return;
            }
            
            // Load background image
            string imagePath = "Assets/PNTG/textures/Numbers/Trace_2_Instruction.JPG";
            var backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            
            if (backgroundSprite == null)
            {
                Debug.LogError($"Background image not found at {imagePath}");
                return;
            }
            
            // Update using SerializedObject
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("backgroundImage").objectReferenceValue = backgroundSprite;
            serializedObject.FindProperty("numberName").stringValue = "2";
            serializedObject.ApplyModifiedProperties();
            
            // Ensure stroke exists with proper control points
            if (config.Strokes == null || config.Strokes.Length == 0)
            {
                config.AddStroke();
            }
            
            if (config.Strokes.Length > 0)
            {
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
            }
            
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Updated Number2Config with background image: {backgroundSprite.name}");
            Debug.Log($"Wobble radius: {config.Strokes[0].WobbleRadius}px");
            
            // Select the asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}