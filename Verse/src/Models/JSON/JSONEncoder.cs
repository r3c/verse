using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Verse.Exceptions;

namespace Verse.Models.JSON
{
	class JSONEncoder<T> : AbstractEncoder<T>
	{
		#region Attributes

		private Dictionary<Type, object>	converters;

        private Encoding					encoding;

        private IFormatter					formatter;

		private Writer						writer;

        #endregion

        #region Constructors

        public	JSONEncoder (Encoding encoding, Dictionary<Type, object> converters, IFormatter formatter)
		{
			this.converters = converters;
			this.encoding = encoding;
			this.formatter = formatter;
			this.writer = null;
		}

		#endregion

    	#region Methods / Public

		public override void	Bind ()
		{
        	object	converter;
        	Type	type;

        	type = typeof (T);

        	if (this.converters.TryGetValue (type, out converter))
        		this.writer = this.BuildWriter (type, converter);
        	else if (type == typeof (bool))
        		this.writer = this.BuildWriter<bool> (JSONConverter.FromBoolean);
        	else if (type == typeof (char))
        		this.writer = this.BuildWriter<char> (JSONConverter.FromChar);
        	else if (type == typeof (float))
        		this.writer = this.BuildWriter<float> (JSONConverter.FromFloat4);
        	else if (type == typeof (double))
        		this.writer = this.BuildWriter<double> (JSONConverter.FromFloat8);
        	else if (type == typeof (sbyte))
        		this.writer = this.BuildWriter<sbyte> (JSONConverter.FromInt1s);
        	else if (type == typeof (byte))
        		this.writer = this.BuildWriter<byte> (JSONConverter.FromInt1u);
        	else if (type == typeof (short))
        		this.writer = this.BuildWriter<short> (JSONConverter.FromInt2s);
        	else if (type == typeof (ushort))
        		this.writer = this.BuildWriter<ushort> (JSONConverter.FromInt2u);
        	else if (type == typeof (int))
        		this.writer = this.BuildWriter<int> (JSONConverter.FromInt4s);
        	else if (type == typeof (uint))
        		this.writer = this.BuildWriter<uint> (JSONConverter.FromInt4u);
        	else if (type == typeof (long))
        		this.writer = this.BuildWriter<long> (JSONConverter.FromInt8s);
        	else if (type == typeof (ulong))
        		this.writer = this.BuildWriter<ulong> (JSONConverter.FromInt8u);
        	else if (type == typeof (string))
        		this.writer = this.BuildWriter<string> (JSONConverter.FromString);
        	else
        		throw new BindTypeException (type, "no converter for this type has been defined");
		}

        public override bool	Encode (Stream stream, T instance)
        {
            using (JSONWriter writer = new JSONWriter (stream, this.encoding))
            {
            	return this.Write (writer, instance);
            }
        }

		public override IEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter)
		{
			throw new NotImplementedException();
		}

		public override IEncoder<U>	HasItems<U> (EncoderArrayGetter<T, U> getter)
		{
			throw new NotImplementedException();
		}

		public override IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter)
		{
			throw new NotImplementedException();
		}

        #endregion

		#region Methods / Private

		private Writer	BuildWriter (Type type, object converter)
		{
			#warning FIXME
			throw new NotImplementedException ();
		}

		private Writer	BuildWriter<U> (WriterImporter<U> importer)
		{
			DynamicMethod				method;
        	WriterImporterWrapper<U>	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONWriter), typeof (WriterImporter<U>), typeof (T)}, this.GetType ());

        	this.GenerateWriter (method, typeof (U), importer.GetType ().GetMethod ("Invoke"));

			wrapper = (WriterImporterWrapper<U>)method.CreateDelegate (typeof (WriterImporterWrapper<U>));

			return (JSONWriter writer, T value) => wrapper (writer, importer, value);
		}

        private void	GenerateWriter (DynamicMethod method, Type type, MethodInfo converter)
        {
        	ILGenerator	generator;

			generator = method.GetILGenerator ();

			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Callvirt, converter);
			generator.Emit (OpCodes.Ret);
        }

		private bool	Write (JSONWriter writer, T value)
		{
			if (this.writer != null)
				return this.writer (writer, value);

			throw new NotImplementedException ();
		}

		#endregion

        #region Types

        private delegate bool	Writer (JSONWriter writer, T value);

        private delegate bool	WriterImporter<U> (JSONWriter writer, U value);

        private delegate bool	WriterImporterWrapper<U> (JSONWriter writer, WriterImporter<U> importer, T value);
        
        #endregion
	}
}
