using UnityEngine;

public class WarpedInCreature : MonoBehaviour
{
	public GameObject warpOutEffectPrefab;

	public FMODAsset warpOutSound;

	public Material warpedMaterial;

	public float overlayFXduration = 4f;

	private float warpInTime = -1f;

	private float warpOutTime = -1f;

	private void Start()
	{
		if (warpInTime < 0f)
		{
			warpInTime = Time.time;
		}
	}

	private void Update()
	{
		if (warpOutTime > 0f && Time.time > warpOutTime)
		{
			WarpOut();
		}
	}

	public void SetLifeTime(float time)
	{
		if (warpInTime < 0f)
		{
			warpInTime = Time.time;
		}
		warpOutTime = warpInTime + time;
	}

	private void WarpOut()
	{
		if (warpOutEffectPrefab != null)
		{
			Utils.SpawnPrefabAt(warpOutEffectPrefab, null, base.transform.position);
		}
		if (warpOutSound != null)
		{
			FMODUWE.PlayOneShot(warpOutSound, base.transform.position);
		}
		Object.Destroy(base.gameObject);
	}

	private void ApplyAndForgetOverlayFX(GameObject targetObj)
	{
		targetObj.AddComponent<VFXOverlayMaterial>().ApplyAndForgetOverlay(warpedMaterial, "VFXOverlay: Warped", Color.clear, overlayFXduration);
	}
}
