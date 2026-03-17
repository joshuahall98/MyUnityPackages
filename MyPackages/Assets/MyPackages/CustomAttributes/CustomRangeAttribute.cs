using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRangeAttribute : PropertyAttribute
{
    public float Min;
    public float Max;

    public CustomRangeAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
}
