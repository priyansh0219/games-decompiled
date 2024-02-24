public interface IManagedUpdateBehaviour : IManagedBehaviour
{
	int managedUpdateIndex { get; set; }

	void ManagedUpdate();
}
