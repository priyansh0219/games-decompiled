namespace WorldStreaming
{
	public interface IStreamer : IPipeline
	{
		bool IsRunning();

		bool UpdateCenter(Int3 position);

		void Unload();
	}
}
