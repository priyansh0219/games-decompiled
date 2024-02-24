using System;
using UnityEngine;

public class VFXController : MonoBehaviour
{
	[Serializable]
	public class VFXEmitter
	{
		public bool spawnOnPlay;

		public bool isdistanceBased;

		public GameObject fx;

		public bool parented = true;

		public Transform parentTransform;

		public Vector3 posOffset;

		public Vector3 eulerOffset;

		public bool fakeParent;

		public bool lateTime;

		[HideInInspector]
		public GameObject instanceGO;

		[HideInInspector]
		public ParticleSystem fxPS;
	}

	public VFXEmitter[] emitters;

	private void SpawnFX(int i)
	{
		if (emitters[i].fx != null)
		{
			Transform transform = (emitters[i].parented ? emitters[i].parentTransform : base.transform);
			GameObject gameObject = Utils.SpawnPrefabAt(emitters[i].fx, transform, transform.position);
			if (emitters[i].fakeParent && emitters[i].parented)
			{
				gameObject.AddComponent<VFXFakeParent>().Parent(emitters[i].parentTransform, emitters[i].posOffset, emitters[i].eulerOffset);
			}
			else
			{
				gameObject.transform.localEulerAngles = emitters[i].eulerOffset;
				gameObject.transform.localPosition = emitters[i].posOffset;
			}
			if (emitters[i].lateTime)
			{
				gameObject.AddComponent<VFXLateTimeParticles>();
			}
			if (!emitters[i].parented)
			{
				gameObject.transform.parent = null;
			}
			emitters[i].instanceGO = gameObject;
			emitters[i].fxPS = gameObject.GetComponent<ParticleSystem>();
			gameObject.SetActive(value: true);
		}
	}

	private void Start()
	{
		for (int i = 0; i < emitters.Length; i++)
		{
			if (!emitters[i].spawnOnPlay)
			{
				SpawnFX(i);
			}
		}
	}

	public void Play(int i)
	{
		if (emitters[i].spawnOnPlay)
		{
			SpawnFX(i);
		}
		if (emitters[i].fxPS != null && !emitters[i].lateTime)
		{
			emitters[i].fxPS.Play();
		}
		if ((bool)emitters[i].instanceGO)
		{
			emitters[i].instanceGO.BroadcastMessage("Play", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Play()
	{
		for (int i = 0; i < emitters.Length; i++)
		{
			Play(i);
		}
	}

	public void Stop(int i)
	{
		if (emitters[i] != null && emitters[i].instanceGO != null)
		{
			if (emitters[i].fxPS != null && !emitters[i].lateTime)
			{
				emitters[i].fxPS.Stop();
			}
			if (emitters[i].isdistanceBased)
			{
				emitters[i].instanceGO.transform.parent = null;
			}
			emitters[i].instanceGO.BroadcastMessage("Stop", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Stop()
	{
		for (int i = 0; i < emitters.Length; i++)
		{
			Stop(i);
		}
	}

	public void StopAndDestroy(int i, float destroyInSeconds)
	{
		Stop(i);
		if (emitters[i] != null)
		{
			UnityEngine.Object.Destroy(emitters[i].instanceGO, destroyInSeconds);
		}
	}

	public void StopAndDestroy(float destroyInSeconds)
	{
		for (int i = 0; i < emitters.Length; i++)
		{
			Stop(i);
			if (emitters[i] != null)
			{
				UnityEngine.Object.Destroy(emitters[i].instanceGO, destroyInSeconds);
			}
		}
	}
}
