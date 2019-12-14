using System;

namespace OpenGL2D.Helpers
{
    public static class HelperGL
    {
        public static float Clamp(float value, float lowerBound, float upperBound)
        {
            if (value > upperBound)
                return upperBound;
            else if (value < lowerBound)
                return lowerBound;
            else
                return value;
        }

        public static float Max(float value, float compareValue)
        {
            return value >= compareValue ? value : compareValue;
        }
    }
}
