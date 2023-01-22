using System;
using System.Collections.Generic;

namespace Verse.Resolvers
{
	/// <summary>
	/// This resolver is used to retrieve appropriate converter for a given value type and entity type, if available. It
	/// is used within <see cref="Linker"/> to trigger "HasValue" linking on current entity type whenever possible.
	/// </summary>
	internal static class AdapterResolver
	{
		private static readonly Dictionary<Type, PropertyResolver> ForDecoder = new()
		{
			{
				typeof(bool),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<bool, object>>>(a => a.ToBoolean)
			},
			{
				typeof(char),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<char, object>>>(a => a.ToCharacter)
			},
			{
				typeof(decimal),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<decimal, object>>>(a => a.ToDecimal)
			},
			{
				typeof(float),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<float, object>>>(a => a.ToFloat32)
			},
			{
				typeof(double),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<double, object>>>(a => a.ToFloat64)
			},
			{
				typeof(sbyte),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<sbyte, object>>>(a => a.ToInteger8S)
			},
			{
				typeof(byte),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<byte, object>>>(a => a.ToInteger8U)
			},
			{
				typeof(short),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<short, object>>>(a => a.ToInteger16S)
			},
			{
				typeof(ushort),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<ushort, object>>>(a => a.ToInteger16U)
			},
			{
				typeof(int),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<int, object>>>(a => a.ToInteger32S)
			},
			{
				typeof(uint),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<uint, object>>>(a => a.ToInteger32U)
			},
			{
				typeof(long),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<long, object>>>(a => a.ToInteger64S)
			},
			{
				typeof(ulong),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<ulong, object>>>(a => a.ToInteger64U)
			},
			{
				typeof(string),
				PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<string, object>>>(a => a.ToString)
			}
		};

		private static readonly Dictionary<Type, PropertyResolver> ForEncoder = new()
		{
			{
				typeof(bool),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<bool, object>>>(a => a.FromBoolean)
			},
			{
				typeof(char),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<char, object>>>(a => a.FromCharacter)
			},
			{
				typeof(decimal),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<decimal, object>>>(a => a.FromDecimal)
			},
			{
				typeof(float),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<float, object>>>(a => a.FromFloat32)
			},
			{
				typeof(double),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<double, object>>>(a => a.FromFloat64)
			},
			{
				typeof(sbyte),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<sbyte, object>>>(a => a.FromInteger8S)
			},
			{
				typeof(byte),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<byte, object>>>(a => a.FromInteger8U)
			},
			{
				typeof(short),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<short, object>>>(a => a.FromInteger16S)
			},
			{
				typeof(ushort),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<ushort, object>>>(a => a.FromInteger16U)
			},
			{
				typeof(int),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<int, object>>>(a => a.FromInteger32S)
			},
			{
				typeof(uint),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<uint, object>>>(a => a.FromInteger32U)
			},
			{
				typeof(long),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<long, object>>>(a => a.FromInteger64S)
			},
			{
				typeof(ulong),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<ulong, object>>>(a => a.FromInteger64U)
			},
			{
				typeof(string),
				PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<string, object>>>(a => a.FromString)
			}
		};

		public static bool TryGetDecoderConverter<TNative, TEntity>(IDecoderAdapter<TNative> adapter,
			out Setter<TEntity, TNative> getter)
		{
			if (!AdapterResolver.ForDecoder.TryGetValue(typeof(TEntity), out var generator))
			{
				getter = default;

				return false;
			}

			var untyped = generator.SetCallerGenericArguments(typeof(TNative)).GetGetter(adapter);

			getter = (Setter<TEntity, TNative>) untyped;

			return true;
		}

		public static bool TryGetEncoderConverter<TNative, TEntity>(IEncoderAdapter<TNative> adapter,
			out Func<TEntity, TNative> converter)
		{
			if (!AdapterResolver.ForEncoder.TryGetValue(typeof(TEntity), out var generator))
			{
				converter = default;

				return false;
			}

			var untyped = generator.SetCallerGenericArguments(typeof(TNative)).GetGetter(adapter);

			converter = (Func<TEntity, TNative>) untyped;

			return true;
		}
	}
}
