using System;

namespace WM.Util
{
    /// <summary>
    /// Math utility functions.
    /// </summary>
    public class UtilMath
    {
        static public readonly double s_factorRadiansToDegrees = 180.0 / Math.PI;
        static public readonly double s_factorDegreesToRadians = Math.PI / 180.0;

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        static public double ToDegrees(double radians)
        {
            return radians * s_factorRadiansToDegrees;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        static public double ToRadians(double degrees)
        {
            return degrees * s_factorDegreesToRadians;
        }
    }
}
