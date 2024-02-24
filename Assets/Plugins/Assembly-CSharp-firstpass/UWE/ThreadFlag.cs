using System;
using System.IO;

namespace UWE
{
	public static class ThreadFlag
	{
		public struct Helper : IDisposable
		{
			private int id;

			public Helper(string label)
			{
				id = Begin(label);
			}

			public void Dispose()
			{
				End(id);
			}
		}

		private static object mutex = new object();

		private static int nextId = 0;

		private static string Id2Filename(int id)
		{
			return SNUtils.InsideDevTemp("thread-" + id + "-stillalive.txt");
		}

		public static int Begin(string label)
		{
			int num = -1;
			lock (mutex)
			{
				num = nextId++;
			}
			using (StreamWriter streamWriter = FileUtils.CreateTextFile(Id2Filename(num)))
			{
				streamWriter.Write(label + DateTime.Now);
				return num;
			}
		}

		public static void End(int id)
		{
			File.Delete(Id2Filename(id));
		}
	}
}
