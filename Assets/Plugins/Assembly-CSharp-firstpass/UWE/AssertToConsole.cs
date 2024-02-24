using System.Collections;

namespace UWE
{
	public static class AssertToConsole
	{
		public static void Fail(string format, params object[] args)
		{
		}

		public static void IsTrue(bool condition, string format, params object[] args)
		{
			if (!condition)
			{
				Fail(format, args);
			}
		}

		public static void IsFalse(bool condition, string format, params object[] args)
		{
			IsTrue(!condition, format, args);
		}

		public static void IsNull(object obj, string format, params object[] args)
		{
			IsTrue(obj == null, format, args);
		}

		public static void IsNotNull(object obj, string format, params object[] args)
		{
			IsTrue(obj != null, format, args);
		}

		public static void IsEmpty(IEnumerable collection, string format, params object[] args)
		{
			IsFalse(collection.GetEnumerator().MoveNext(), format, args);
		}

		public static void IsNotEmpty(IEnumerable collection, string format, params object[] args)
		{
			IsTrue(collection.GetEnumerator().MoveNext(), format, args);
		}

		public static void Less(int a, int b, string format, params object[] args)
		{
			IsTrue(a < b, format, args);
		}

		public static void AreEqual(object a, object b, string format, params object[] args)
		{
			if (a == null)
			{
				IsTrue(b == null, format, args);
			}
			else
			{
				IsTrue(a.Equals(b), format, args);
			}
		}
	}
}
