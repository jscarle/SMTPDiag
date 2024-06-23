namespace SMTPDiag3.Common;

internal static class StringExtensions
{
    public static string[] SplitLines(this string str)
    {
        return str.Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n');
    }
}
