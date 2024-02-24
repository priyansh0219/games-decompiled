using Gendarme;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing
{
	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public abstract class PostProcessingComponentCommandBuffer<T> : PostProcessingComponent<T> where T : PostProcessingModel
	{
		public abstract CameraEvent GetCameraEvent();

		public abstract string GetName();

		public abstract void PopulateCommandBuffer(CommandBuffer cb);
	}
}
