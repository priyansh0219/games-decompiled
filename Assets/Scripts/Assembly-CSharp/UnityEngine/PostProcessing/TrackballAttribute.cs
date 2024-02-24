using Gendarme;

namespace UnityEngine.PostProcessing
{
	[SuppressMessage("Gendarme.Rules.Naming", "UseCorrectSuffixRule")]
	public sealed class TrackballAttribute : PropertyAttribute
	{
		public readonly string method;

		public TrackballAttribute(string method)
		{
			this.method = method;
		}
	}
}
