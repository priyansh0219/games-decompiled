using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class ReefbackCreature : MonoBehaviour, GameObjectPool.IPooledObject
{
	private Transform slot;

	private Creature creature;

	private void Start()
	{
		creature = GetComponent<Creature>();
		slot = base.transform.parent;
	}

	private void Update()
	{
		if (creature != null && slot != null)
		{
			creature.leashPosition = slot.position;
		}
	}

	private void OnRespawnerSpawned(Respawn respawner)
	{
		respawner.addComponents.Add(typeof(ReefbackCreature).AssemblyQualifiedName);
	}

	private void OnExamine()
	{
		Object.Destroy(this);
	}

	public void Despawn(float time = 0f)
	{
		Object.Destroy(this);
	}

	public void Spawn(float time = 0f, bool active = true)
	{
	}
}
