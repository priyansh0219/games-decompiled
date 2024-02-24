public interface IObstacle
{
	bool IsDeconstructionObstacle();

	bool CanDeconstruct(out string reason);
}
