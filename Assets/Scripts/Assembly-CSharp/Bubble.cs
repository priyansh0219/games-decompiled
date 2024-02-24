using UWE;
using UnityEngine;

public class Bubble : MonoBehaviour
{
	public GameObject popParticlePrefab;

	public GameObject firstPersonPopParticlePrefab;

	public float oxygenSeconds = 10f;

	[AssertNotNull]
	public FMODAsset popSound;

	[AssertNotNull]
	public FMODAsset popOnPlayerSound;

	public float dontPopTime;

	private bool hasPopped;

	private Rigidbody rigidBody;

	private void Awake()
	{
		rigidBody = GetComponent<Rigidbody>();
		dontPopTime = Time.time + 0.4f;
		rigidBody.velocity = Vector3.zero;
	}

	private void Update()
	{
		if (!(Ocean.main == null) && base.transform.position.y >= Ocean.GetOceanLevel())
		{
			Pop();
		}
	}

	private void OnCollisionEnter(Collision collisionInfo)
	{
		if (hasPopped || (Time.time < dontPopTime && collisionInfo.gameObject.layer != LayerMask.NameToLayer("Player")))
		{
			return;
		}
		bool flag = false;
		OxygenManager component = collisionInfo.gameObject.GetComponent<OxygenManager>();
		if ((bool)component)
		{
			component.AddOxygen(oxygenSeconds);
			flag = Player.main != null && Player.main.gameObject == collisionInfo.gameObject;
			if (flag && (bool)firstPersonPopParticlePrefab)
			{
				Player.main.PlayOneShotPS(firstPersonPopParticlePrefab);
			}
		}
		Pop(flag);
	}

	public void Pop(bool hitPlayer = false)
	{
		if (!hasPopped)
		{
			if (hitPlayer)
			{
				Utils.PlayFMODAsset(popOnPlayerSound, Utils.GetLocalPlayerPos());
			}
			else
			{
				Utils.PlayFMODAsset(popSound, base.transform.position);
			}
			UWE.Utils.DestroyWrap(base.gameObject);
			hasPopped = true;
		}
	}
}
