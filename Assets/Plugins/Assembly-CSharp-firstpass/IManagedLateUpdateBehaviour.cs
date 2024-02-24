public interface IManagedLateUpdateBehaviour : IManagedBehaviour
{
	int managedLateUpdateIndex { get; set; }

	void ManagedLateUpdate();
}
