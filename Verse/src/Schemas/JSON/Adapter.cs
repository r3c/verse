using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.ParserDescriptors;

namespace Verse.Schemas.JSON
{
    class Adapter : RecurseParserDescriptor.IAdapter<Value>
    {
    	#region Attributes

    	private readonly Dictionary<Type, object>	converters = new Dictionary<Type, object>
    	{
    		// FIXME: should handle automatic conversions here
    		{typeof (bool),		new Converter<Value, bool> ((v) => v.type == FieldType.Boolean && v.boolean)},
    		{typeof (sbyte),	new Converter<Value, sbyte> ((v) => v.type == FieldType.Number ? unchecked ((sbyte)v.number) : (sbyte)0)},
    		{typeof (byte),		new Converter<Value, byte> ((v) => v.type == FieldType.Number ? unchecked ((byte)v.number) : (byte)0)},
    		{typeof (short),	new Converter<Value, short> ((v) => v.type == FieldType.Number ? unchecked ((short)v.number) : (short)0)},
    		{typeof (ushort),	new Converter<Value, ushort> ((v) => v.type == FieldType.Number ? unchecked ((ushort)v.number) : (ushort)0)},
    		{typeof (int),		new Converter<Value, int> ((v) => v.type == FieldType.Number ? unchecked ((int)v.number) : 0)},
    		{typeof (uint),		new Converter<Value, uint> ((v) => v.type == FieldType.Number ? unchecked ((uint)v.number) : 0)},
    		{typeof (long),		new Converter<Value, long> ((v) => v.type == FieldType.Number ? unchecked ((long)v.number) : 0)},
    		{typeof (ulong),	new Converter<Value, ulong> ((v) => v.type == FieldType.Number ? unchecked ((ulong)v.number) : 0)},
    		{typeof (float),	new Converter<Value, float> ((v) => v.type == FieldType.Number ? unchecked ((float)v.number) : 0)},
    		{typeof (double),	new Converter<Value, double> ((v) => v.type == FieldType.Number ? v.number : 0)},
    		{typeof (decimal),	new Converter<Value, decimal> ((v) => v.type == FieldType.Number ? unchecked ((decimal)v.number) : 0)},
    		{typeof (char),		new Converter<Value, char> ((v) => v.type == FieldType.String && v.str.Length > 0 ? v.str[0] : '\0')},
            {typeof (string),	new Converter<Value, string> ((v) => v.type == FieldType.String ? v.str : string.Empty)}
    	};

    	#endregion

    	#region Methods

		public Converter<Value, T> Get<T> ()
		{
			object	box;

			if (!this.converters.TryGetValue (typeof (T), out box))
				throw new InvalidCastException (string.Format (CultureInfo.InvariantCulture, "no available converter from JSON value to type '{0}'", typeof (T)));

			return (Converter<Value, T>)box;
		}

		public void Set<T> (Converter<Value, T> converter)
		{
			if (converter == null)
				throw new ArgumentNullException ("converter");

			this.converters[typeof (T)] = converter;
		}

		#endregion
    }
}
