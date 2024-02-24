using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gendarme;

namespace UWE
{
	[SuppressMessage("Subnautica.Rules", "ValueTypeEnumeratorRule")]
	public class StreamWrapper : IEnumerable<char>, IEnumerable, IEnumerator<char>, IEnumerator, IDisposable
	{
		private readonly Stream stream;

		private int current = -1;

		public char Current => (char)current;

		object IEnumerator.Current => current;

		public StreamWrapper(Stream stream)
		{
			this.stream = stream;
		}

		public IEnumerator<char> GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			current = stream.ReadByte();
			return current >= 0;
		}

		public void Reset()
		{
			stream.Seek(0L, SeekOrigin.Begin);
		}

		public void Dispose()
		{
			stream.Dispose();
		}
	}
}
