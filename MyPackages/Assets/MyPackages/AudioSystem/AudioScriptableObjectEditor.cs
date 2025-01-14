#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioScriptableObject))]
public class AudioScriptableObjectEditor : Editor
{
    SerializedProperty volumeRollOffCurve;
    SerializedProperty volumeRollOffMode;
    SerializedProperty maxDistance;
    SerializedProperty minDistance;

    float previousMaxDistance;

    private void OnEnable()
    {

        volumeRollOffCurve = serializedObject.FindProperty("volumeRollOffCurve");
        volumeRollOffMode = serializedObject.FindProperty("volumeRollOffMode");
        maxDistance = serializedObject.FindProperty("maxDistance");
        minDistance = serializedObject.FindProperty("minDistance");

        // Ensure the property has an assigned AnimationCurve with at least 2 keys
        if (volumeRollOffCurve.animationCurveValue == null || volumeRollOffCurve.animationCurveValue.keys.Length <= 1)
        {
            volumeRollOffCurve.animationCurveValue = new AnimationCurve(
                new Keyframe(minDistance.floatValue, 1),
                new Keyframe(maxDistance.floatValue, 0)
            );

            serializedObject.ApplyModifiedProperties();
        }

        previousMaxDistance = maxDistance.floatValue;
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        var audioScriptableObject = (AudioScriptableObject)target;

        // Draw properties except volumeRollOffCurve, volumeRollOffMode, maxDistance, and minDistance
        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true); // Move to the first visible property

        do
        {
            if (property.name != "volumeRollOffCurve" &&
                property.name != "volumeRollOffMode" &&
                property.name != "maxDistance" &&
                property.name != "minDistance")
            {
                EditorGUILayout.PropertyField(property, true);
            }
        } while (property.NextVisible(false));

        // Draw the volume roll-off related properties
        EditorGUILayout.PropertyField(volumeRollOffMode);
        EditorGUILayout.PropertyField(minDistance);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(maxDistance);
        if (EditorGUI.EndChangeCheck())
        {

            // Scale the curve if maxDistance has changed
            if (maxDistance.floatValue != previousMaxDistance)
            {

                volumeRollOffCurve.animationCurveValue = ScaleCurve(volumeRollOffCurve.animationCurveValue, maxDistance.floatValue);
                previousMaxDistance = maxDistance.floatValue; // Update the previous maxDistance

                serializedObject.ApplyModifiedProperties();

            }    
        }

        // Draw the custom curve field if the mode is custom
        if (audioScriptableObject.volumeRollOffMode == AudioRolloffMode.Custom)
        {

            Rect bbox = new Rect(0, 0, maxDistance.floatValue, 1);

            EditorGUI.BeginChangeCheck();
            volumeRollOffCurve.animationCurveValue = EditorGUILayout.CurveField(new GUIContent("Volume RollOff Curve"), volumeRollOffCurve.animationCurveValue, Color.green, bbox);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private AnimationCurve ScaleCurve(AnimationCurve curve, float newMaxDistance)
    {

        float scaleFactor = newMaxDistance / previousMaxDistance;
        AnimationCurve scaledCurve = new AnimationCurve();

        foreach (var key in curve.keys)
        {
            Keyframe scaledKey = key;
            scaledKey.time *= scaleFactor;
            scaledKey.inTangent /= scaleFactor;
            scaledKey.outTangent /= scaleFactor;
            scaledCurve.AddKey(scaledKey);
        }

        return scaledCurve;
    }
}
#endif

