using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ColoredFieldAttribute : PropertyAttribute
{
    public enum PresetColors
    {
        Sound
    }

    public Color Color { get; }

    public ColoredFieldAttribute(float r, float g, float b)
    {
        Color = new Color(r, g, b);
    }

    public ColoredFieldAttribute(PresetColors color)
    {
        Color = GetColorFromEnum(color);
    }

    private Color GetColorFromEnum(PresetColors color)
    {
        switch (color)
        {
            case PresetColors.Sound:
                return new Color(0f, .3f, 0f);
            default:
                return Color.white; // Default color if nothing matches
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ColoredFieldAttribute))]
public class ColoredFieldDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        // Account for child properties if expanded
        if (property.isExpanded)
        {
            var childProperty = property.Copy();
            var endProperty = property.GetEndProperty();

            while (childProperty.NextVisible(true) && !SerializedProperty.EqualContents(childProperty, endProperty))
            {
                // Split the property path into segments
                string[] pathSegments = childProperty.propertyPath.Split('.');

                // Check if the path has more than one segment, meaning it's inside a container
                if (pathSegments.Length > 2)
                {
                    // If there are more than two segments, it's a nested class/field
                    // Here we just skip the nested properties
                    continue;
                }

                height += EditorGUI.GetPropertyHeight(childProperty, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ColoredFieldAttribute colorAttribute = (ColoredFieldAttribute)attribute;

        // Draw highlight behind the parent label
        Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.DrawRect(labelRect, colorAttribute.Color);

        // Draw the foldout with default Unity text color
        property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float childY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty childProperty = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            if (childProperty.NextVisible(true))
            {
                do
                {
                    // Stop if we exceed the parent's scope
                    if (SerializedProperty.EqualContents(childProperty, endProperty))
                        break;

                    float childHeight = EditorGUI.GetPropertyHeight(childProperty, true);
                    Rect childPosition = new Rect(position.x, childY, position.width, childHeight);

                    // Draw highlight behind each child property
                    EditorGUI.DrawRect(childPosition, colorAttribute.Color);

                    // Draw the child property
                    EditorGUI.PropertyField(childPosition, childProperty, true);

                    // Move down for the next property
                    childY += childHeight + EditorGUIUtility.standardVerticalSpacing;

                } while (childProperty.NextVisible(false)); // false ensures we iterate within this foldout
            }

            EditorGUI.indentLevel--;
        }
    }

}
#endif
