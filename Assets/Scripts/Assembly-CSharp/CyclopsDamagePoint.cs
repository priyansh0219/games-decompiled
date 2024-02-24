using UnityEngine;

[RequireComponent(typeof(WeldablePoint))]
public class CyclopsDamagePoint : HandTarget
{
	[AssertNotNull]
	public LiveMixin liveMixin;

	private ParticleSystem ps;

	private CyclopsExternalDamageManager manager;

	public void SetManager(CyclopsExternalDamageManager mng)
	{
		manager = mng;
	}

	public void SpawnFx(GameObject prefabGo)
	{
		GameObject gameObject = Utils.SpawnZeroedAt(prefabGo, base.transform);
		ps = gameObject.GetComponent<ParticleSystem>();
		ps.Play();
	}

	public void OnRepair()
	{
		if (ps != null)
		{
			ps.transform.parent = null;
			ps.Stop();
			Object.Destroy(ps.gameObject, 3f);
		}
		manager.RepairPoint(this);
		base.gameObject.SetActive(value: false);
	}

	public void RestoreHealth()
	{
		liveMixin.health = 1f;
	}

	private void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "CyclopsDamagePoint", translate: true);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "CyclopsDamagePointRepair", translate: true);
		HandReticle.main.SetProgress(liveMixin.GetHealthFraction());
		HandReticle.main.SetIcon(HandReticle.IconType.Progress);
	}
}
