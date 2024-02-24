using Gendarme;

namespace UnityEngine.PostProcessing
{
	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public abstract class PostProcessingComponentRenderTexture<T> : PostProcessingComponent<T> where T : PostProcessingModel
	{
		public virtual void Prepare(Material material)
		{
		}
	}
}
