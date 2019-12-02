using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LinearInterpolation
{

    public static float Lerp(float start, float end, float timeStartedLerping, float lerpTime = 1) //Easier usage of lerp
    {
        float timeSinceStarted = Time.time - timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;
        float result = Mathf.Lerp(start, end, percentageComplete);
        return result;
    }
    public static Vector3 LerpV3(Vector3 startPosition, Vector3 endPosition, float timeStartedLerping, float lerpTime = 1) //Easier usage of 3D lerp
    {
        float timeSinceStarted = Time.time - timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;
        Vector3 result = Vector3.Lerp(startPosition, endPosition, percentageComplete);
        return result;
    }


}
