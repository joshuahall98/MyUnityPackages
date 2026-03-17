using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionField;

    public ShowIfAttribute(string conditionField)
    {
        ConditionField = conditionField;
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);

        if (conditionProperty == null)
        {
            Debug.LogWarning($"ShowIf: Could not find property '{showIf.ConditionField}'");
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        if (conditionProperty.propertyType == SerializedPropertyType.Boolean && !conditionProperty.boolValue)
        {
            return; // Hide the property
        }

        var clampedRangeHandled = HandleClampedRange(property, ref position, ref label);

        if (!clampedRangeHandled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }



    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);

        if (conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.Boolean && !conditionProperty.boolValue)
        {
            return 0; // Hide the property
        }

        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    private bool HandleClampedRange(SerializedProperty property, ref Rect position, ref GUIContent label)
    {
        // Check if ClampedRange exists
        CustomRangeAttribute range = fieldInfo.GetCustomAttribute<CustomRangeAttribute>();

        if (range != null)
        {
            // If ClampedRange is present, draw a slider instead of a standard field
            if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = EditorGUI.Slider(position, label, property.floatValue, range.Min, range.Max);
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.IntSlider(position, label, property.intValue, (int)range.Min, (int)range.Max);
            }

            return true;
        }
        
        return false;
    }
}
#endif

