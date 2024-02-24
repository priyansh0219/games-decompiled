using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Grabcrab : Creature
{
	public enum State
	{
		RunningTo = 0,
		PickingUp = 1,
		RunningHome = 2,
		PuttingDown = 3,
		IdleAtHome = 4
	}

	public GameObject model;

	public GrabcrabHome home;

	public float runSpeed;

	public float carryingSpeed;

	public float pickupPutdownTime;

	private Pickupable alertItem;

	public State state = State.IdleAtHome;

	public float timeLastStateChange;

	public Vector3 holdingOffset = new Vector3(0f, 0f, 1f);

	public float timeOfNextIdleSound;

	public AudioClip pickupClip;

	public AudioClip putdownClip;

	public AudioClip scareClip;

	public bool IsFree()
	{
		return alertItem == null;
	}

	public void AlertToItem(Pickupable pickupable)
	{
		if ((bool)pickupable && alertItem == null)
		{
			alertItem = pickupable;
			Debug.DrawLine(base.transform.position, pickupable.gameObject.transform.position, Color.white, 1f);
		}
	}

	private void UpdateState()
	{
		if (state == State.IdleAtHome && (bool)alertItem)
		{
			SetState(State.RunningTo);
		}
		else if (state == State.RunningTo)
		{
			if ((bool)alertItem && alertItem.isPickupable)
			{
				if (new Vector3(base.transform.position.x - alertItem.transform.position.x, 0f, base.transform.position.z - alertItem.transform.position.z).magnitude <= 1.5f)
				{
					SetState(State.PickingUp);
				}
			}
			else
			{
				alertItem = null;
				SetState(State.RunningHome);
			}
		}
		else if (state == State.PickingUp)
		{
			if (Time.time - timeLastStateChange >= pickupPutdownTime)
			{
				alertItem.transform.parent = base.transform;
				alertItem.transform.localPosition = holdingOffset;
				SetState(State.RunningHome);
			}
		}
		else if (state == State.PuttingDown)
		{
			if (Time.time - timeLastStateChange >= pickupPutdownTime)
			{
				DropItem();
				SetState(State.IdleAtHome);
			}
		}
		else if (state == State.RunningHome && home != null && new Vector3(base.transform.position.x - home.transform.position.x, 0f, base.transform.position.z - home.transform.position.z).magnitude <= 1f)
		{
			SetState(State.PuttingDown);
		}
	}

	private void DropItem()
	{
		if (alertItem != null && alertItem.transform.parent == base.gameObject.transform)
		{
			alertItem.transform.parent = null;
			alertItem = null;
		}
	}

	private void SetState(State newState)
	{
		if (newState != state)
		{
			state = newState;
			timeLastStateChange = Time.time;
			AudioSource component = base.gameObject.GetComponent<AudioSource>();
			if (state == State.PickingUp)
			{
				component.clip = pickupClip;
				component.Play();
				SetNextIdleTime();
			}
			else if (state == State.PuttingDown)
			{
				component.clip = putdownClip;
				component.Play();
				SetNextIdleTime();
			}
		}
	}

	private void MoveCrab()
	{
		Vector3 vector = Vector3.zero;
		float num = 0f;
		if (state == State.RunningTo)
		{
			if (alertItem != null)
			{
				vector = alertItem.transform.position;
			}
			num = runSpeed;
		}
		else if (state == State.RunningHome && home != null)
		{
			vector = home.transform.position;
			num = carryingSpeed;
		}
		if (vector != Vector3.zero)
		{
			float t = num * Time.deltaTime / (vector - base.transform.position).magnitude;
			base.transform.position = Vector3.Lerp(base.transform.position, vector, t);
			base.transform.LookAt(vector);
			PutCrabOnTerrain();
		}
	}

	public void PutCrabOnTerrain()
	{
		SphereCollider component = base.gameObject.GetComponent<SphereCollider>();
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(base.transform.position, Vector3.down, out hitInfo, 2f) && hitInfo.collider.gameObject != base.gameObject)
		{
			base.transform.position = new Vector3(base.transform.position.x, hitInfo.point.y + component.radius, base.transform.position.z);
		}
	}

	public void Update()
	{
		MoveCrab();
		UpdateState();
		if (Time.time > timeOfNextIdleSound)
		{
			SetNextIdleTime();
		}
	}

	private void SetNextIdleTime()
	{
		timeOfNextIdleSound = Time.time + (float)Random.Range(3, 7);
	}

	private void OnTouch(GameObject obj)
	{
		if ((bool)obj.GetComponent<Player>())
		{
			bool flag = true;
			if (state == State.PickingUp)
			{
				SetState(State.RunningHome);
			}
			else if (state == State.RunningHome)
			{
				DropItem();
			}
			else
			{
				flag = false;
			}
			if (flag)
			{
				AudioSource component = base.gameObject.GetComponent<AudioSource>();
				component.clip = scareClip;
				component.Play();
			}
		}
	}
}
