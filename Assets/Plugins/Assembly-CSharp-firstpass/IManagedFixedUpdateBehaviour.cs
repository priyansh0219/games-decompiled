public interface IManagedFixedUpdateBehaviour : IManagedBehaviour
{
	int managedFixedUpdateIndex { get; set; }

	void ManagedFixedUpdate();
}
