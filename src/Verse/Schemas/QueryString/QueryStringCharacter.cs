
namespace Verse.Schemas.QueryString;

internal static class QueryStringCharacter
{
    private static readonly int[] Hexadecimals = new int[128];

    private static readonly bool[] Unreserved = new bool[128];

    static QueryStringCharacter()
    {
        for (int i = 0; i < Hexadecimals.Length; ++i)
            Hexadecimals[i] = -1;

        for (int c = '0'; c <= '9'; ++c)
            Hexadecimals[c] = c - '0';

        for (int c = 'A'; c <= 'F'; ++c)
            Hexadecimals[c] = c - 'A' + 10;

        for (int c = 'a'; c <= 'f'; ++c)
            Hexadecimals[c] = c - 'a' + 10;

        for (int c = '0'; c <= '9'; ++c)
            Unreserved[c] = true;

        for (int c = 'A'; c <= 'Z'; ++c)
            Unreserved[c] = true;

        for (int c = 'a'; c <= 'z'; ++c)
            Unreserved[c] = true;

        Unreserved['-'] = true;
        Unreserved['_'] = true;
        Unreserved['.'] = true;
        Unreserved['!'] = true;
        Unreserved['~'] = true;
        Unreserved['*'] = true;
        Unreserved['\''] = true;
        Unreserved['('] = true;
        Unreserved[')'] = true;
        Unreserved[','] = true;
        Unreserved['"'] = true;
        Unreserved['$'] = true;
        Unreserved[':'] = true;
        Unreserved['@'] = true;
        Unreserved['/'] = true;
        Unreserved['?'] = true;
    }

    /// <summary>
    /// Return decimal value of given hexadecimal character.
    /// </summary>
    /// <param name="character">Hexadecimal character</param>
    /// <returns>Decial value, or -1 if character was not a valid
    /// hexadecimal digit</returns>
    public static int HexaToDecimal(int character)
    {
        return character > Hexadecimals.Length ? -1 : Hexadecimals[character];
    }

    /// <summary>
    /// Check if character is a parameters separator (& or ;).
    /// </summary>
    /// <param name="character">Input character</param>
    /// <returns>True if character is a separator, false otherwise</returns>
    public static bool IsSeparator(int character)
    {
        return character == '&' || character == ';';
    }

    /// <summary>
    /// Check if character is unreserved, i.e. can be used in a query
    /// string without having to escape it.
    /// </summary>
    /// <param name="character">Input character</param>
    /// <remarks>Array is not supported yet (",")</remarks>
    /// <returns>True if character is unreserved, false otherwise</returns>
    public static bool IsUnreserved(int character)
    {
        return character >= 0 && character < Unreserved.Length && Unreserved[character];
    }
}