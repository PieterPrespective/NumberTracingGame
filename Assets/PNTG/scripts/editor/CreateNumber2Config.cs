using UnityEngine;
using UnityEditor;

namespace PNTG.Editor
{
    /// <summary>
    /// Editor utility to create Number2 configuration ScriptableObject
    /// </summary>
    public static class CreateNumber2Config
    {
        [MenuItem("PNTG/Create Number 2 Config")]
        public static void CreateNumber2Configuration()
        {
            // Create Number2 ScriptableObject
            var config = ScriptableObject.CreateInstance<NumberConfiguration>();
            
            // Set basic properties
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("numberName").stringValue = "2";
            serializedObject.FindProperty("scoreValue").intValue = 1;
            serializedObject.ApplyModifiedProperties();
            
            // Add stroke with control points for number 2
            config.AddStroke();
            var stroke = config.Strokes[0];
            
            // Define control points for number 2 curve
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
            
            stroke.SetControlPoints(controlPoints);
            
            // Create directory if needed
            if (!AssetDatabase.IsValidFolder("Assets/PNTG/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets/PNTG", "ScriptableObjects");
            }
            
            // Save the asset
            string path = "Assets/PNTG/ScriptableObjects/Number2Config.asset";
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            
            EditorUtility.SetDirty(config);
            
            Debug.Log($"Number 2 configuration created at: {path}");
            Debug.Log($"Default wobble radius: {stroke.WobbleRadius}px");
            
            // Select the created asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}