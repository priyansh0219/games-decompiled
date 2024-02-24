using UnityEngine;

public static class BehaviourUpdateUtils
{
	private delegate BehaviourUpdateManager.BehaviourSet<T> GetBehaviourSet<T>(BehaviourUpdateManager manager) where T : IManagedBehaviour;

	public static bool Register(IManagedUpdateBehaviour behaviour)
	{
		return RegisterForUpdate(behaviour);
	}

	public static bool Register(IManagedLateUpdateBehaviour behaviour)
	{
		return RegisterForLateUpdate(behaviour);
	}

	public static bool Register(IManagedFixedUpdateBehaviour behaviour)
	{
		return RegisterForFixedUpdate(behaviour);
	}

	public static bool RegisterForUpdate(IManagedUpdateBehaviour behaviour)
	{
		return Register(behaviour, (BehaviourUpdateManager manager) => manager.updateSet);
	}

	public static bool RegisterForLateUpdate(IManagedLateUpdateBehaviour behaviour)
	{
		return Register(behaviour, (BehaviourUpdateManager manager) => manager.lateUpdateSet);
	}

	public static bool RegisterForFixedUpdate(IManagedFixedUpdateBehaviour behaviour)
	{
		return Register(behaviour, (BehaviourUpdateManager manager) => manager.fixedUpdateSet);
	}

	public static bool Deregister(IManagedUpdateBehaviour behaviour)
	{
		return DeregisterFromUpdate(behaviour);
	}

	public static bool Deregister(IManagedLateUpdateBehaviour behaviour)
	{
		return DeregisterFromLateUpdate(behaviour);
	}

	public static bool Deregister(IManagedFixedUpdateBehaviour behaviour)
	{
		return DeregisterFromFixedUpdate(behaviour);
	}

	public static bool DeregisterFromUpdate(IManagedUpdateBehaviour behaviour)
	{
		return Deregister(behaviour, (BehaviourUpdateManager manager) => manager.updateSet);
	}

	public static bool DeregisterFromLateUpdate(IManagedLateUpdateBehaviour behaviour)
	{
		return Deregister(behaviour, (BehaviourUpdateManager manager) => manager.lateUpdateSet);
	}

	public static bool DeregisterFromFixedUpdate(IManagedFixedUpdateBehaviour behaviour)
	{
		return Deregister(behaviour, (BehaviourUpdateManager manager) => manager.fixedUpdateSet);
	}

	private static bool Register<T>(T behaviour, GetBehaviourSet<T> getBehaviourSet) where T : IManagedBehaviour
	{
		BehaviourUpdateManager instance = BehaviourUpdateManager.Instance;
		if (!instance)
		{
			Debug.LogErrorFormat("BehaviourUpdateManager must exist when IManagedBehaviours like '{0}' are enabled!", behaviour.GetProfileTag());
			return false;
		}
		return getBehaviourSet(instance).Add(behaviour);
	}

	private static bool Deregister<T>(T behaviour, GetBehaviourSet<T> getBehaviourSet) where T : IManagedBehaviour
	{
		BehaviourUpdateManager instance = BehaviourUpdateManager.Instance;
		if (!instance)
		{
			return false;
		}
		return getBehaviourSet(instance).Remove(behaviour);
	}
}
