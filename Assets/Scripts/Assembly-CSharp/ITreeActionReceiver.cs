public interface ITreeActionReceiver
{
	bool inProgress { get; }

	float progress { get; }

	bool PerformAction(TechType techType);
}
