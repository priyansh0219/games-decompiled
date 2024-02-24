using System.ComponentModel;

namespace Oculus.Platform
{
	public enum KeyValuePairType : uint
	{
		[Description("STRING")]
		String = 0u,
		[Description("INT")]
		Int = 1u,
		[Description("DOUBLE")]
		Double = 2u
	}
}
