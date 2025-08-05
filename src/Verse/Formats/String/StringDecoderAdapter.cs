﻿using System;
using System.Globalization;

namespace Verse.Formats.String;

internal class StringDecoderAdapter : IDecoderAdapter<string>
{
    public static readonly StringDecoderAdapter Instance = new();

    public Func<string, bool> Boolean => source => bool.TryParse(source, out var target)
        ? target
        : false;

    public Func<string, char> Character => source => source.Length > 0
        ? source[0]
        : '\0';

    public Func<string, decimal> Decimal => source =>
        decimal.TryParse(source, NumberStyles.Number, CultureInfo.InvariantCulture, out var target)
            ? target
            : 0;

    public Func<string, float> Float32 => source =>
        float.TryParse(source, NumberStyles.Number, CultureInfo.InvariantCulture, out var target)
            ? target
            : 0;

    public Func<string, double> Float64 => source =>
        double.TryParse(source, NumberStyles.Number, CultureInfo.InvariantCulture, out var target)
            ? target
            : 0;

    public Func<string, sbyte> Integer8S => source => sbyte.TryParse(source, out var target)
        ? target
        : default;

    public Func<string, byte> Integer8U => source => byte.TryParse(source, out var target)
        ? target
        : default;

    public Func<string, short> Integer16S => source => short.TryParse(source, out var target)
        ? target
        : default;

    public Func<string, ushort> Integer16U => source => ushort.TryParse(source, out var target)
        ? target
        : default;

    public Func<string, int> Integer32S => source => int.TryParse(source, out var target)
        ? target
        : 0;

    public Func<string, uint> Integer32U => source => uint.TryParse(source, out var target)
        ? target
        : 0;

    public Func<string, long> Integer64S => source => long.TryParse(source, out var target)
        ? target
        : 0;

    public Func<string, ulong> Integer64U => source => ulong.TryParse(source, out var target)
        ? target
        : 0;

    public Func<string, string> String => source => source;
}