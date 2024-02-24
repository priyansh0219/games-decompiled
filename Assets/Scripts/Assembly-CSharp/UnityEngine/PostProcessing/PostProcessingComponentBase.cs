using Gendarme;

namespace UnityEngine.PostProcessing
{
	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	[SuppressMessage("Gendarme.Rules.Naming", "AvoidRedundancyInTypeNameRule")]
	public abstract class PostProcessingComponentBase
	{
		public PostProcessingContext context;

		public abstract bool active { get; }

		public virtual DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.None;
		}

		public virtual void OnEnable()
		{
		}

		public virtual void OnDisable()
		{
		}

		public abstract PostProcessingModel GetModel();
	}
}
