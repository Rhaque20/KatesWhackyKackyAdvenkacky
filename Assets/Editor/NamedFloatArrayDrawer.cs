using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedFloatArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
        try
        {
            string fieldName = ((NamedArrayAttribute)attribute).names[pos];
            property.floatValue = EditorGUI.FloatField(rect, fieldName, property.floatValue, EditorStyles.numberField);
        }
        catch
        {
            property.floatValue = EditorGUI.FloatField(rect, "Element " + pos.ToString(), property.floatValue, EditorStyles.numberField);
        }

    }
}