using UnityEngine;
using UnityEditor;

namespace PNTG.Editor
{
    /// <summary>
    /// Simple script to create number configurations
    /// </summary>
    [InitializeOnLoad]
    public class CreateConfigsScript
    {
        static CreateConfigsScript()
        {
            // This will run when the editor starts
            EditorApplication.delayCall += CreateConfigs;
        }
        
        private static void CreateConfigs()
        {
            EditorApplication.delayCall -= CreateConfigs;
            
            // Create configs only once
            if (!AssetDatabase.IsValidFolder("Assets/PNTG/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets/PNTG", "ScriptableObjects");
            }
            
            // Create Number 2 config as example
            CreateNumber2ConfigAsset();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static void CreateNumber2ConfigAsset()
        {
            string path = "Assets/PNTG/ScriptableObjects/Number2Config.asset";
            
            // Check if already exists
            if (AssetDatabase.LoadAssetAtPath<NumberConfiguration>(path) != null)
            {
                Debug.Log("Number2Config already exists");
                return;
            }
            
            var config = ScriptableObject.CreateInstance<NumberConfiguration>();
            
            // Set basic properties using reflection to access private fields
            var type = typeof(NumberConfiguration);
            var numberNameField = type.GetField("numberName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scoreValueField = type.GetField("scoreValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var backgroundImageField = type.GetField("backgroundImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (numberNameField != null) numberNameField.SetValue(config, "2");
            if (scoreValueField != null) scoreValueField.SetValue(config, 1);
            
            // Load background image
            var backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PNTG/textures/Numbers/Trace_2_Instruction.JPG");
            if (backgroundImageField != null && backgroundSprite != null)
            {
                backgroundImageField.SetValue(config, backgroundSprite);
            }
            
            // Add stroke
            config.AddStroke();
            if (config.Strokes.Length > 0)
            {
                var stroke = config.Strokes[0];
                var controlPoints = new NURBSControlPoint[]
                {
                    new NURBSControlPoint(new Vector2(150, 150), 1f),
                    new NURBSControlPoint(new Vector2(300, 100), 1f),
                    new NURBSControlPoint(new Vector2(450, 150), 1f),
                    new NURBSControlPoint(new Vector2(500, 200), 1f),
                    new NURBSControlPoint(new Vector2(450, 250), 1f),
                    new NURBSControlPoint(new Vector2(350, 300), 1f),
                    new NURBSControlPoint(new Vector2(250, 400), 1f),
                    new NURBSControlPoint(new Vector2(150, 450), 1f),
                    new NURBSControlPoint(new Vector2(150, 500), 1f),
                    new NURBSControlPoint(new Vector2(300, 500), 1f),
                    new NURBSControlPoint(new Vector2(450, 500), 1f)
                };
                
                stroke.SetControlPoints(controlPoints);
            }
            
            AssetDatabase.CreateAsset(config, path);
            EditorUtility.SetDirty(config);
            
            Debug.Log($"Created Number2Config at {path}");
        }
    }
}