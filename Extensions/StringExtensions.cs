namespace TelegramBot.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Returns substring with start index to end index (without last one)
    /// </summary>
    /// <param name="str"></param>
    /// <param name="startIndex">start index</param>
    /// <param name="endIndex">stop index</param>
    /// <returns></returns>
    public static string MySubstring(this string str, int startIndex, int endIndex)
    {
        var result = "";
        for (var i = startIndex; i < endIndex; i++)
        {
            result += str[i];
        }

        return result;
    }
}