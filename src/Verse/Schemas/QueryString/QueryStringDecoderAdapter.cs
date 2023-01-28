using System.Globalization;

namespace Verse.Schemas.QueryString;

internal class QueryStringDecoderAdapter : IDecoderAdapter<string>
{
    public Setter<bool, string> Boolean => (ref bool target, string source) => bool.TryParse(source, out target);

    public Setter<char, string> Character => (ref char target, string source) =>
        target = source.Length > 0 ? source[0] : default;

    public Setter<decimal, string> Decimal => (ref decimal target, string source) =>
        decimal.TryParse(source, NumberStyles.Number, CultureInfo.InvariantCulture, out target);

    public Setter<float, string> Float32 => (ref float target, string source) =>
        float.TryParse(source, NumberStyles.Number, CultureInfo.InvariantCulture, out target);

    public Setter<double, string> Float64 => (ref double target, string source) =>
        double.TryParse(source, NumberStyles.Number, CultureInfo.InvariantCulture, out target);

    public Setter<sbyte, string> Integer8S => (ref sbyte target, string source) =>
        sbyte.TryParse(source, out target);

    public Setter<byte, string> Integer8U =>
        (ref byte target, string source) => byte.TryParse(source, out target);

    public Setter<short, string> Integer16S => (ref short target, string source) =>
        short.TryParse(source, out target);

    public Setter<ushort, string> Integer16U => (ref ushort target, string source) =>
        ushort.TryParse(source, out target);

    public Setter<int, string> Integer32S => (ref int target, string source) => int.TryParse(source, out target);

    public Setter<uint, string> Integer32U => (ref uint target, string source) =>
        uint.TryParse(source, out target);

    public Setter<long, string> Integer64S => (ref long target, string source) =>
        long.TryParse(source, out target);

    public Setter<ulong, string> Integer64U => (ref ulong target, string source) =>
        ulong.TryParse(source, out target);

    public Setter<string, string> String => (ref string target, string source) => target = source;
}