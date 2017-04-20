using System;
using System.Text;
using Verse.DecoderDescriptors.Abstract;
using Verse.DecoderDescriptors.Flat;

namespace Verse.Schemas.QueryString
{
	static class Reader
	{
		private static readonly bool[] unreserved = new bool[128];

		static Reader()
		{
			for (int c = '0'; c <= '9'; ++c)
				Reader.unreserved[c] = true;

			for (int c = 'A'; c <= 'Z'; ++c)
				Reader.unreserved[c] = true;

			for (int c = 'a'; c <= 'z'; ++c)
				Reader.unreserved[c] = true;

			Reader.unreserved['-'] = true;
			Reader.unreserved['_'] = true;
			Reader.unreserved['.'] = true;
			Reader.unreserved['!'] = true;
			Reader.unreserved['~'] = true;
			Reader.unreserved['*'] = true;
			Reader.unreserved['\''] = true;
			Reader.unreserved['('] = true;
			Reader.unreserved[')'] = true;
			Reader.unreserved[','] = true;
			Reader.unreserved['"'] = true;
			Reader.unreserved['$'] = true;
			Reader.unreserved[':'] = true;
			Reader.unreserved['@'] = true;
			Reader.unreserved['/'] = true;
			Reader.unreserved['?'] = true;
		}

		/// <summary>
		/// Check if character is a parameters separator (& or ;).
		/// </summary>
		/// <param name="c">Input character</param>
		/// <returns>True if character is a separator, false otherwise</returns>
		public static bool IsSeparator(int c)
		{
			return c == '&' || c == ';';
		}

		/// <summary>
		/// Check if character is unreserved, i.e. can be used in a query
		/// string without having to escape it.
		/// </summary>
		/// <param name="c">Input character</param>
		/// <remarks>Array is not supported yet (",")</remarks>
		/// <returns>True if character is unreserved, false otherwise</returns>
		public static bool IsUnreserved(int c)
		{
			return c >= 0 && c < Reader.unreserved.Length && Reader.unreserved[c];
		}
	}

	class Reader<TEntity> : FlatReader<TEntity, ReaderState, string>
	{
		#region Methods / Public

		public override FlatReader<TOther, ReaderState, string> Create<TOther>()
		{
			return new Reader<TOther>();
		}

		public override bool ReadEntity(Func<TEntity> constructor, ReaderState state, out TEntity entity)
		{
			entity = constructor();

			while (state.Current != -1)
			{
				bool isKeyEmpty;
				EntityTree<TEntity, ReaderState> node;

				node = this.fields;
				isKeyEmpty = true;

				if (Reader.IsUnreserved(state.Current))
				{
					node = node.Follow((char)state.Current);

					isKeyEmpty = false;

					state.Pull();
				}

				while (Reader.IsUnreserved(state.Current))
				{
					node = node.Follow((char)state.Current);

					state.Pull();
				}

				if (isKeyEmpty)
				{
					state.Error("empty field");

					entity = default(TEntity);

					return false;
				}

				if (state.Current == '=')
				{
					string dummy;

					state.Pull();

					if (node.Read == null)
						this.ReadFieldValue(state, out dummy);
					else if (!node.Read(ref entity, state))
						return false;
				}

				if (state.Current == -1)
					break;

				if (!Reader.IsSeparator(state.Current))
				{
					state.Error("unexpected character");

					return false;
				}

				state.Pull();
			}

			return true;
		}

		public override bool ReadValue(ReaderState state, out TEntity target)
		{
			string value;

			if (!this.ReadFieldValue(state, out value))
			{
				target = default(TEntity);

				return false;
			}

			if (this.converter != null)
				target = this.converter(value);
			else
				target = default(TEntity);

			return true;
		}

		#endregion

		#region Methods / Private

		private bool ReadFieldValue(ReaderState state, out string value)
		{
			var builder = new StringBuilder(32);

			while (state.Current != -1)
			{
				int current = state.Current;

				if (Reader.IsUnreserved(current) || current == '%')
					builder.Append((char)current);
				else if (current == '+')
					builder.Append(' ');
				else
					break;

				state.Pull();
			}

			value = Uri.UnescapeDataString(builder.ToString());

			return true;
		}

		#endregion
	}
}
