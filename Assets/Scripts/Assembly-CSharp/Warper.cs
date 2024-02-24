using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Warper : Creature
{
	[AssertNotNull]
	public GameObject warpOutEffectPrefab;

	[AssertNotNull]
	public GameObject warpInEffectPrefab;

	[AssertNotNull]
	public Material warpedMaterial;

	public float overlayFXduration = 4f;

	public FMOD_StudioEventEmitter warpOutSound;

	public FMOD_StudioEventEmitter warpInSound;

	public const float kAttackInfectionThreshold = 0.33f;

	private const float warpOutDuration = 0.73f;

	private const float warpInDuration = 0.73f;

	private WarperSpawner spawner;

	public void WarpIn(WarperSpawner spawner)
	{
		this.spawner = spawner;
		Utils.SpawnPrefabAt(warpInEffectPrefab, null, base.transform.position);
		ApplyAndForgetOverlayFX(base.gameObject);
		if (warpInSound != null)
		{
			Utils.PlayEnvSound(warpInSound, base.transform.position);
		}
	}

	public void WarpOut()
	{
		SafeAnimator.SetBool(GetAnimator(), "warping", value: true);
		Invoke("EndWarpOut", 0.73f);
	}

	public override void OnKill()
	{
		Object.Destroy(spawner);
		base.OnKill();
	}

	private void EndWarpOut()
	{
		Utils.SpawnPrefabAt(warpOutEffectPrefab, null, base.transform.position);
		ApplyAndForgetOverlayFX(base.gameObject);
		if (warpOutSound != null)
		{
			Utils.PlayEnvSound(warpOutSound, base.transform.position);
		}
		if (spawner != null)
		{
			spawner.OnWarpOut();
		}
		Object.Destroy(base.gameObject);
	}

	private void ApplyAndForgetOverlayFX(GameObject targetObj)
	{
		targetObj.AddComponent<VFXOverlayMaterial>().ApplyAndForgetOverlay(warpedMaterial, "VFXOverlay: Warped", Color.clear, overlayFXduration);
	}
}
