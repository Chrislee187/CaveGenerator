using System;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment NOTE: Can't use compound assignment in this ver. of unity


public static class For
{
    private static readonly Action<int, int> InnerNop = (i, i1) => { };
    private static readonly Action<int> OuterNop = (i) => { };

    /// <summary>
    /// Helper to avoid ugly nested if's, using LESS THAN operations for the for loop
    /// </summary>
    /// <param name="maxX">Loop until the 'x' value is LESS THAN this value</param>
    /// <param name="maxY">Loop until the 'y' value is LESS THAN this value</param>
    /// <param name="innerAction"></param>
    /// <param name="outerAction"></param>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="stepX"></param>
    /// <param name="stepY"></param>
    public static void Xy(
        int maxX, int maxY,
        Action<int, int> innerAction = null,
        Action<int> outerAction = null,
        int minX = 0, int minY = 0,
        int stepX = 1, int stepY = 1)
        => Xy(minX, maxX, stepX, minY, maxY, stepY, innerAction, outerAction);

    public static void Xy(
        int minX, int maxX, int stepX,
        int minY, int maxY, int stepY,
        Action<int, int> innerAction = null,
        Action<int> outerAction = null
    )
    {
        innerAction = innerAction ?? InnerNop;
        outerAction = outerAction ?? OuterNop;


        for (var y = minY; y < maxY; y += stepY)
        {
            outerAction(y);
            for (var x = minX; x < maxX; x += stepX)
            {
                innerAction(x, y);
            }
        }
    }

    /// <summary>
    /// Helper to avoid ugly nested if's
    /// </summary>
    public static void Yx(
        int maxX, int maxY,
        Action<int, int> innerAction = null,
        Action<int> outerAction = null,
        int minX = 0, int minY = 0,
        int stepX = 1, int stepY = 1)
    {
        innerAction = innerAction ?? InnerNop;
        outerAction = outerAction ?? OuterNop;

        for (var x = minX; x < maxX; x += stepX)
        {
            outerAction(x);
            for (var y = minY; y < maxY; y += stepY)
            {
                innerAction(x, y);
            }
        }
    }
}