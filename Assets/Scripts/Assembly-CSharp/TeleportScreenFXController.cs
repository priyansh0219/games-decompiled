using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(TeleportScreenFX))]
public class TeleportScreenFXController : MonoBehaviour
{
	public float transitionSpeedEnter = 1.5f;

	public float transitionSpeedExit = 0.6f;

	private TeleportScreenFX fx;

	private bool isFadingIn;

	private void OnEnable()
	{
		PrecursorTeleporter.TeleportEventStart += StartTeleport;
		PrecursorTeleporter.TeleportEventEnd += StopTeleport;
	}

	private void OnDisable()
	{
		PrecursorTeleporter.TeleportEventStart -= StartTeleport;
		PrecursorTeleporter.TeleportEventEnd -= StopTeleport;
	}

	private void Start()
	{
		fx = GetComponent<TeleportScreenFX>();
		if (Player.main == null)
		{
			Object.Destroy(this);
			Object.Destroy(GetComponent<TeleportScreenFX>());
		}
		fx.amount = 0f;
	}

	public void StartTeleport()
	{
		isFadingIn = true;
	}

	public void StopTeleport()
	{
		isFadingIn = false;
	}

	private void Update()
	{
		if (isFadingIn)
		{
			fx.amount = Mathf.MoveTowards(fx.amount, 1f, Time.deltaTime * transitionSpeedEnter);
		}
		else
		{
			fx.amount = Mathf.MoveTowards(fx.amount, 0f, Time.deltaTime * transitionSpeedExit);
		}
		if (fx.amount > 0f && !fx.enabled)
		{
			fx.enabled = true;
		}
	}
}
