using Gendarme;

namespace UnityEngine.PostProcessing
{
	[SuppressMessage("Gendarme.Rules.Naming", "UseCorrectSuffixRule")]
	public sealed class MinAttribute : PropertyAttribute
	{
		public readonly float min;

		public MinAttribute(float min)
		{
			this.min = min;
		}
	}
}
