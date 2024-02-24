using UnityEngine;

public class VFXSubLeakPoint : MonoBehaviour
{
	public float waterlevel;

	public GameObject[] leakEffectPrefabs;

	private bool _pointActive;

	[AssertNotNull]
	public Renderer[] childRends;

	[AssertNotNull]
	public VFXLerpColor[] childLerps;

	private GameObject leakEffect;

	private VFXWaterSpray waterSpray;

	public bool pointActive
	{
		get
		{
			return _pointActive;
		}
		set
		{
			_pointActive = value;
			if (_pointActive)
			{
				Play();
			}
			else
			{
				Stop();
			}
		}
	}

	[ContextMenu("Upgrade Configuration")]
	public void UpgradeConfiguration()
	{
		childRends = GetComponentsInChildren<Renderer>(includeInactive: true);
		childLerps = GetComponentsInChildren<VFXLerpColor>(includeInactive: true);
	}

	public void Start()
	{
		if (!pointActive)
		{
			DisableRenderers();
		}
	}

	private void Play()
	{
		CancelInvoke("DisableRenderers");
		for (int i = 0; i < childLerps.Length; i++)
		{
			childLerps[i].ResetColor();
		}
		for (int j = 0; j < childRends.Length; j++)
		{
			childRends[j].enabled = true;
		}
		if (leakEffect == null)
		{
			int num = leakEffectPrefabs.Length;
			GameObject gameObject = leakEffectPrefabs[Random.Range(0, num - 1)];
			if (gameObject != null)
			{
				leakEffect = Object.Instantiate(gameObject);
				Transform obj = leakEffect.transform;
				obj.parent = base.transform;
				Vector3 localPosition = (obj.localEulerAngles = Vector3.zero);
				obj.localPosition = localPosition;
			}
		}
		if (base.transform.position.y <= 0f && leakEffect != null)
		{
			waterSpray = leakEffect.GetComponent<VFXWaterSpray>();
			waterSpray.Play();
		}
	}

	private void DisableRenderers()
	{
		if (childLerps != null)
		{
			for (int i = 0; i < childLerps.Length; i++)
			{
				childLerps[i].ResetColor();
			}
		}
		if (childRends == null)
		{
			return;
		}
		for (int j = 0; j < childRends.Length; j++)
		{
			if (childRends[j] != null)
			{
				childRends[j].enabled = false;
			}
		}
	}

	private void Stop()
	{
		float num = 0f;
		if (childLerps != null)
		{
			for (int i = 0; i < childLerps.Length; i++)
			{
				if (childLerps[i].duration > num)
				{
					num = childLerps[i].duration;
				}
				childLerps[i].Play();
			}
		}
		if (waterSpray != null)
		{
			waterSpray.Stop();
		}
		if ((bool)leakEffect)
		{
			Object.Destroy(leakEffect, 1.4f);
			leakEffect = null;
		}
		Invoke("DisableRenderers", num);
	}

	public void UpdateEffects()
	{
		if (!(waterSpray == null))
		{
			waterSpray.waterlevel = waterlevel;
			if (base.transform.position.y > 0f && waterSpray.GetIsPlaying())
			{
				waterSpray.Stop();
			}
		}
	}
}
