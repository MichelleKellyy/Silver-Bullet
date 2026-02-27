using UnityEngine;

public class OddRangeAttribute : PropertyAttribute
{
    public readonly int min;
    public readonly int max;

    public OddRangeAttribute(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}