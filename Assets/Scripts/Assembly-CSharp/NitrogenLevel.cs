using ProtoBuf;
using UWE;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(Rigidbody))]
[ProtoContract]
public class NitrogenLevel : MonoBehaviour
{
	[ProtoMember(1)]
	public float nitrogenLevel;

	public AnimationCurve depthCurve = new AnimationCurve();

	public float safeNitrogenDepth;

	public float kBreathScalar = 10f;

	public float kDissipateScalar = 2f;

	private bool nitrogenEnabled;

	private float kDepthInterval = 50f;

	private void Start()
	{
		Player component = Utils.GetLocalPlayer().GetComponent<Player>();
		component.tookBreathEvent.AddHandler(this, OnTookBreath);
		component.playerRespawnEvent.AddHandler(base.gameObject, OnRespawn);
		DevConsole.RegisterConsoleCommand(this, "nitrogen");
	}

	private void OnConsoleCommand_nitrogen(NotificationCenter.Notification n)
	{
		nitrogenEnabled = !nitrogenEnabled;
		ErrorMessage.AddDebug("nitrogen now " + nitrogenEnabled);
	}

	public void OnTookBreath(Player player)
	{
		if (nitrogenEnabled)
		{
			float depthOf = Ocean.GetDepthOf(player.gameObject);
			float num = depthCurve.Evaluate(depthOf / 2048f);
			safeNitrogenDepth = UWE.Utils.Slerp(safeNitrogenDepth, depthOf, num * kBreathScalar);
		}
	}

	private void OnRespawn(Player p)
	{
		nitrogenLevel = 0f;
	}

	public bool GetNitrogenEnabled()
	{
		return nitrogenEnabled;
	}

	public float GetNitrogenLevel()
	{
		if (!nitrogenEnabled)
		{
			return 0f;
		}
		return safeNitrogenDepth;
	}

	public bool GetLevelsDangerous()
	{
		return Ocean.GetDepthOf(Player.main.gameObject) < safeNitrogenDepth - kDepthInterval;
	}

	private void Update()
	{
		if (nitrogenEnabled)
		{
			float depthOf = Ocean.GetDepthOf(Player.main.gameObject);
			if (GetLevelsDangerous() && Random.value < 0.01f)
			{
				Player.main.gameObject.GetComponent<LiveMixin>().TakeDamage(1f + Random.value * (safeNitrogenDepth - depthOf) / kDepthInterval);
			}
			if (depthOf < safeNitrogenDepth)
			{
				float num = 1f;
				num = ((Player.main.motorMode != Player.MotorMode.Dive) ? (num * 2f) : (Mathf.Clamp(2f - GetComponent<Rigidbody>().velocity.magnitude, 0f, 2f) * 1f));
				safeNitrogenDepth = UWE.Utils.Slerp(safeNitrogenDepth, depthOf, kDissipateScalar * num * Time.deltaTime);
			}
		}
	}
}
