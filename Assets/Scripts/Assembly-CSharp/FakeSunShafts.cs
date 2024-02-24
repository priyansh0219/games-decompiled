using UnityEngine;

public class FakeSunShafts : MonoBehaviour
{
	public Color beamColor;

	public float quantity = 20f;

	public float seaLevelOffset;

	private GameObject player;

	private ParticleSystem shaft_ps;

	private void UpdateChanges()
	{
		if (quantity != shaft_ps.emissionRate)
		{
			shaft_ps.emissionRate = quantity;
		}
		if (shaft_ps.startColor != beamColor)
		{
			shaft_ps.startColor = beamColor;
			beamColor = shaft_ps.startColor;
		}
	}

	private void Start()
	{
		shaft_ps = GetComponent<ParticleSystem>();
		player = Utils.GetLocalPlayer();
	}

	private void Update()
	{
		UpdateChanges();
		base.transform.position = new Vector3(player.transform.position.x, seaLevelOffset, player.transform.position.z);
	}
}
