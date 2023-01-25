using J4JSoftware.Logging;

namespace J4JMapLibrary;

internal static class MapExtensions
{
    internal static T ConformValueToRange<T>(T toCheck, T min, T max, string name, IJ4JLogger logger)
        where T : IComparable<T>
    {
        if (toCheck.CompareTo(min) < 0)
        {
            logger.Warning("{0} ({1}) < minimum ({2}), capping", name, toCheck, min);
            return min;
        }

        if (toCheck.CompareTo(max) <= 0)
            return toCheck;

        logger.Warning("{0} ({1}) > maximum ({2}), capping", name, toCheck, max);
        return max;
    }
}
