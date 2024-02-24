using UWE;
using UnityEngine;

public class Gene : MonoBehaviour
{
	public string geneName;

	public string geneDescription;

	private string geneMaterial;

	public Event<float> onChangedEvent = new Event<float>();

	private float scalar;

	private float targetScalar;

	private bool expiring;

	public float Scalar
	{
		get
		{
			return scalar;
		}
		set
		{
			targetScalar = Mathf.Clamp01(value);
		}
	}

	public void MarkExpired()
	{
		expiring = true;
	}

	public bool GetExpiring()
	{
		return expiring;
	}

	public void SetMaterial(string nonPlayerMaterial, string playerMaterial)
	{
		string text = (base.gameObject.CompareTag("Player") ? playerMaterial : nonPlayerMaterial);
		Debug.Log(GetType().ToString() + ".SetMaterial(" + text);
		if (Utils.ModifyMaterial(base.gameObject, text))
		{
			geneMaterial = text;
		}
	}

	protected virtual void OnDestroy()
	{
		if (geneMaterial != "")
		{
			Debug.Log("Destroying gene " + geneName + ", removing material " + geneMaterial);
			Utils.ModifyMaterial(base.gameObject, geneMaterial, addShader: false);
		}
		onChangedEvent.Trigger(0f);
	}

	private void Update()
	{
		if (expiring)
		{
			scalar = UWE.Utils.Slerp(scalar, 0f, Time.deltaTime / 2f);
			onChangedEvent.Trigger(scalar);
			if (Utils.NearlyEqual(scalar, 0f))
			{
				Object.Destroy(this);
			}
		}
		else if (!Utils.NearlyEqual(scalar, targetScalar))
		{
			scalar = UWE.Utils.Slerp(scalar, targetScalar, Time.deltaTime / 4f);
			onChangedEvent.Trigger(scalar);
		}
	}
}
