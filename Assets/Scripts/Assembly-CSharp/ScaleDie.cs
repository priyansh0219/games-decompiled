using UnityEngine;

public class ScaleDie : MonoBehaviour
{
	public MeshRenderer meshRenderer;

	private float growTime = 0.1f;

	private float startTime;

	private float lifeTime = 1f;

	public EcoEvent ecoEvent;

	private void Start()
	{
		startTime = Time.time;
		meshRenderer.enabled = base.enabled;
		Invoke("OnExpire", lifeTime);
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - startTime) / growTime) * 2f * ecoEvent.GetRange();
		base.gameObject.transform.localScale = new Vector3(num, num, num);
	}

	private void OnExpire()
	{
		Object.Destroy(base.gameObject);
	}
}
