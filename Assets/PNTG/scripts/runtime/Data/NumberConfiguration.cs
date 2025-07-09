using UnityEngine;

namespace PNTG
{
    /// <summary>
    /// ScriptableObject that holds configuration data for a single number or shape
    /// </summary>
    [CreateAssetMenu(fileName = "NumberConfig", menuName = "PNTG/Number Configuration", order = 1)]
    public class NumberConfiguration : ScriptableObject
    {
        [SerializeField] private string numberName = "0";
        [SerializeField] private Sprite backgroundImage;
        [SerializeField] private StrokeData[] strokes;
        [SerializeField] private int scoreValue = 1;
        
        public string NumberName => numberName;
        public Sprite BackgroundImage => backgroundImage;
        public StrokeData[] Strokes => strokes;
        public int ScoreValue => scoreValue;
        
        private void OnValidate()
        {
            if (strokes == null)
            {
                strokes = new StrokeData[0];
            }
        }
        
        public void AddStroke()
        {
            var newStrokes = new StrokeData[strokes.Length + 1];
            System.Array.Copy(strokes, newStrokes, strokes.Length);
            newStrokes[strokes.Length] = new StrokeData();
            strokes = newStrokes;
        }
        
        public void RemoveStroke(int index)
        {
            if (index >= 0 && index < strokes.Length)
            {
                var newStrokes = new StrokeData[strokes.Length - 1];
                int destIndex = 0;
                for (int i = 0; i < strokes.Length; i++)
                {
                    if (i != index)
                    {
                        newStrokes[destIndex++] = strokes[i];
                    }
                }
                strokes = newStrokes;
            }
        }
    }
}