using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OddRangeAttribute))]
public class OddRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var oddRange = (OddRangeAttribute)attribute;

        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.HelpBox(position, "[OddRange] only works on int fields.", MessageType.Error);
            return;
        }

        int min = oddRange.min;
        int max = oddRange.max;

        if (max < min) (min, max) = (max, min);

        // Make range odd-friendly
        if (min % 2 == 0) min += 1;
        if (max % 2 == 0) max -= 1;

        if (min > max)
        {
            EditorGUI.HelpBox(position, "OddRange has no valid odd numbers in this range.", MessageType.Error);
            return;
        }

        // Snap current value to odd + clamp
        int value = property.intValue;
        value = Mathf.Clamp(value, min, max);
        if (value % 2 == 0) value += 1;
        value = Mathf.Clamp(value, min, max);

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        // Draw slider using REAL values (but we step manually)
        int newValue = EditorGUI.IntSlider(position, label, value, min, max);

        // Snap whatever the slider gives to the nearest odd number
        if (newValue % 2 == 0)
        {
            // pick nearest odd (prefer lower if equally close)
            int lower = newValue - 1;
            int upper = newValue + 1;

            if (lower < min) newValue = upper;
            else if (upper > max) newValue = lower;
            else newValue = lower;
        }

        if (EditorGUI.EndChangeCheck())
        {
            property.intValue = newValue;
        }

        EditorGUI.EndProperty();
    }
}