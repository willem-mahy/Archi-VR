namespace WM
{
    public class UtilIterate
    {
        /// <summary>
        /// Clamps value to range [minValue, maxValue[ by making it cycle, if necessary.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int MakeCycle(int value, int minValue, int maxValue)
        {
            if (value < minValue)
                return maxValue - 1;

            if (value >= maxValue)
                return minValue;

            return value; // in range [minValue, maxValue[
        }
    }
}
