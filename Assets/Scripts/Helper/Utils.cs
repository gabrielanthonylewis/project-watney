using UnityEngine;

public static class Utils
{
    public static bool Approximately(float a, float b, float threshold)
    {
        return ((a - b) < 0.0f ? ((a - b) * -1.0f) : (a - b)) <= threshold;
    }

    // Convert into [-180, 180] range as this is how it's seen in the inspector.
    public static float ConvertTo180Degrees(float value)
    {
        if (value > 180.0f)
            value -= 360.0f;
        if (value < -180.0f)
            value += 360.0f;
    
        return value;
    }

    public static Vector3 ConvertTo180Degrees(Vector3 value)
    {
        return new Vector3(Utils.ConvertTo180Degrees(value.x),
            Utils.ConvertTo180Degrees(value.y),
            Utils.ConvertTo180Degrees(value.z));
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180.0f)
            angle -= 360.0f;
        if (angle < -180.0f)
            angle += 360.0f;

        return Mathf.Clamp(angle, min, max);
    }
}
