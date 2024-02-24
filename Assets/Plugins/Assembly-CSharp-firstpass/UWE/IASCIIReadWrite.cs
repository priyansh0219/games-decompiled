using System.IO;

namespace UWE
{
	public interface IASCIIReadWrite
	{
		void Read(StreamReader reader);

		void Write(StreamWriter writer);
	}
}
