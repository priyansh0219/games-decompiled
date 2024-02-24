using FMODUnity;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Workbench : GhostCrafter
{
	public Animator animator;

	[AssertNotNull]
	public Transform soundOrigin;

	[AssertNotNull]
	public FMODAsset openSound;

	[AssertNotNull]
	public FMODAsset closeSound;

	[AssertNotNull]
	public StudioEventEmitter fabricateSound;

	public GameObject[] fxLaserBeam;

	public GameObject fxSparksPrefab;

	public GameObject workingLight;

	private GameObject[] fxSparksInstances;

	protected override void Start()
	{
		base.Start();
		InitializeSparkInstances();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (!(base.logic != null) || !base.logic.inProgress)
		{
			return;
		}
		for (int i = 0; i < fxSparksInstances.Length; i++)
		{
			GameObject gameObject = fxSparksInstances[i];
			if (gameObject != null)
			{
				GameObject gameObject2 = fxLaserBeam[i];
				if (gameObject2 != null)
				{
					Transform transform = gameObject2.transform;
					Transform transform2 = ((ghost != null && ghost.itemSpawnPoint != null) ? ghost.itemSpawnPoint : base.transform);
					gameObject.transform.position = GetBeamEnd(transform.position, transform.forward, transform2.position, transform2.up);
				}
			}
		}
	}

	protected override void OnStateChanged(bool crafting)
	{
		if (animator != null)
		{
			animator.SetBool(AnimatorHashID.working, crafting);
		}
		if (fabricateSound.IsPlaying() != crafting)
		{
			if (crafting)
			{
				fabricateSound.Play();
			}
			else
			{
				fabricateSound.Stop();
			}
		}
		for (int i = 0; i < fxLaserBeam.Length; i++)
		{
			GameObject gameObject = fxLaserBeam[i];
			if (gameObject != null)
			{
				gameObject.SetActive(crafting);
			}
		}
		if (fxSparksInstances == null)
		{
			InitializeSparkInstances();
		}
		for (int j = 0; j < fxSparksInstances.Length; j++)
		{
			GameObject gameObject2 = fxSparksInstances[j];
			if (!(gameObject2 != null))
			{
				continue;
			}
			ParticleSystem component = gameObject2.GetComponent<ParticleSystem>();
			if (component != null)
			{
				if (crafting)
				{
					component.Play();
				}
				else
				{
					component.Stop();
				}
			}
		}
		workingLight.SetActive(crafting);
	}

	protected override void OnOpenedChanged(bool opened)
	{
		base.OnOpenedChanged(opened);
		if (animator != null)
		{
			animator.SetBool(AnimatorHashID.open_workbench, opened);
		}
		FMODUWE.PlayOneShot(opened ? openSound : closeSound, soundOrigin.position);
	}

	private static Vector3 GetBeamEnd(Vector3 beamPos, Vector3 beamRot, Vector3 basePos, Vector3 baseRot)
	{
		return beamPos + Vector3.Normalize(beamRot) * (Vector3.Dot(basePos - beamPos, baseRot) / Vector3.Dot(beamRot, baseRot));
	}

	private void InitializeSparkInstances()
	{
		Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);
		fxSparksInstances = new GameObject[4];
		for (int i = 0; i < 4; i++)
		{
			GameObject gameObject = Utils.SpawnZeroedAt(fxSparksPrefab, base.transform);
			gameObject.transform.rotation = rotation;
			fxSparksInstances[i] = gameObject;
		}
	}
}
