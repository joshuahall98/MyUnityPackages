using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HideMinimumDistanceValueAttribute : PropertyAttribute
{
    public AudioRolloffMode rolloffMode;

    public HideMinimumDistanceValueAttribute(AudioRolloffMode mode)
    {
        rolloffMode = mode;
    }
}


#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(HideMinimumDistanceValueAttribute))]
public class HideMinimumDistanceValueDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideMinimumDistanceValueAttribute attr = attribute as HideMinimumDistanceValueAttribute;
        var component = property.serializedObject.targetObject as AudioScriptableObject;


        // Get the enum value from the serialized property
        if (component.volumeRollOffMode == attr.rolloffMode)
            return; // Exit if enum value doesn't match

        // Otherwise, draw the property as usual
        EditorGUI.PropertyField(position, property, label);
    }
}

#endif
