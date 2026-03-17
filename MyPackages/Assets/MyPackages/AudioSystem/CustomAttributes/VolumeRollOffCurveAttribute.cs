using UnityEngine;

// These compiler flags let you put the script anywhere in your project
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Forces an AnimationCurve to keep its keys within a certain window. The default is a range of (x0, y0) = (0, 0) to (x1, y1) = (1, 1).
/// </summary>
/*public class VolumeRollOffCurveAttribute : PropertyAttribute
{

    public AudioRolloffMode rolloffMode;

    public VolumeRollOffCurveAttribute(AudioRolloffMode mode)
    {
        rolloffMode = mode;
    }
}*/



/*#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(VolumeRollOffCurveAttribute))]
public class VolumeRollOffCurveDrawer : PropertyDrawer
{
    float previousMaxDistance;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
       // property.serializedObject.Update();
        //EditorGUI.BeginChangeCheck();
        

        VolumeRollOffCurveAttribute curveAttr = attribute as VolumeRollOffCurveAttribute;
        var component = property.serializedObject.targetObject as AudioScriptableObject;

       // property.animationCurveValue = component.volumeRollOffCurve;

        // Get the enum value from the serialized property
        if (component.volumeRollOffMode != curveAttr.rolloffMode)
            return; // Exit if enum value doesn't match

        // Ensure this attribute is only used with AnimationCurves
        if (property.propertyType != SerializedPropertyType.AnimationCurve)
        {
            EditorGUI.LabelField(position, label.text, "Use [VolumeRollOffCurve] with AnimationCurve.");
            return;
        }

        if (component == null)
        {
            EditorGUI.LabelField(position, label.text, "Component not found.");
            return;
        }

        // Get bbox from the component
        Rect bbox = new Rect(0, 0, component.maxDistance, 1);

        // Ensure the property has an assigned AnimationCurve with at least 2 keys
        if (property.animationCurveValue == null || property.animationCurveValue.keys.Length <= 1)
        {
            property.animationCurveValue = new AnimationCurve(
                new Keyframe(component.minDistance, 1),
                new Keyframe(component.maxDistance, 0)
            );

            previousMaxDistance = component.maxDistance;

            *//*property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);*//*
        }

        var currentMaxDistance = component.maxDistance;

        if (previousMaxDistance != currentMaxDistance)
        {

            property.animationCurveValue = ScaleCurve(component.volumeRollOffCurve, currentMaxDistance);
            previousMaxDistance = currentMaxDistance;

            *//*property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);*//*

        }

        // Draw the curve field
        property.animationCurveValue = EditorGUI.CurveField(position, label, property.animationCurveValue, Color.green, bbox);

       // component.volumeRollOffCurve = property.animationCurveValue;

        *//*EditorGUI.BeginChangeCheck();
        var newCurve = EditorGUI.CurveField(position, label, property.animationCurveValue, Color.green, bbox);
        if (EditorGUI.EndChangeCheck())
        {
            property.animationCurveValue = newCurve;
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }*//*

    }

    private AnimationCurve ScaleCurve(AnimationCurve curve, float newMaxDistance)
    {
        float scaleFactor = newMaxDistance / previousMaxDistance;
        AnimationCurve scaledCurve = new AnimationCurve();

        for (int i = 0; i < curve.keys.Length; i++)
        {
            Keyframe key = curve.keys[i];

            key.time *= scaleFactor;

            // Adjust tangents proportionally to maintain the curve's shape
            key.inTangent /= scaleFactor;
            key.outTangent /= scaleFactor;

            scaledCurve.AddKey(key);

        }

        return scaledCurve;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2.5f; // Adjust the height to fit the label and the curve field
    }
}*/


