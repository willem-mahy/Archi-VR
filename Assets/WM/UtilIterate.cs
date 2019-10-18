namespace WM
{
    class UtilIterate
    {
        // Clamps value to range [minValue, maxValue[ by making it cycle, if necessary.
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
