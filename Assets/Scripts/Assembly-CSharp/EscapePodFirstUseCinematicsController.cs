using UnityEngine;

public class EscapePodFirstUseCinematicsController : MonoBehaviour
{
	[AssertNotNull]
	public EscapePod escapePod;

	[AssertNotNull]
	public Rigidbody escapePodRigidbody;

	[AssertNotNull]
	public CinematicModeTrigger bottomCinematicTarget;

	[AssertNotNull]
	public CinematicModeTrigger bottomFirstUseCinematicTarget;

	[AssertNotNull]
	public CinematicModeTrigger topCinematicTarget;

	[AssertNotNull]
	public CinematicModeTrigger topFirstUseCinematicTarget;

	[AssertNotNull]
	public GameObject skyrayPrefab;

	[AssertNotNull]
	public GameObject rabbitrayPrefab;

	[AssertNotNull]
	public GameObject holefishPrefab;

	[AssertNotNull]
	public Transform skyrayAttachPoint;

	[AssertNotNull]
	public Transform rabbitrayAttachPoint;

	[AssertNotNull]
	public Transform holefish1AttachPoint;

	[AssertNotNull]
	public Transform holefish2AttachPoint;

	private bool initialized;

	private GameObject skyray;

	private GameObject rabbitray;

	private GameObject holefish1;

	private GameObject holefish2;

	private void OnSceneObjectsLoaded()
	{
		if (!initialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		bool flag = !escapePod.bottomHatchUsed;
		bottomCinematicTarget.gameObject.SetActive(!flag);
		bottomFirstUseCinematicTarget.gameObject.SetActive(flag);
		if (flag)
		{
			bottomFirstUseCinematicTarget.onCinematicStart.AddListener(OnBottomHatchCinematicStart);
			bottomFirstUseCinematicTarget.onCinematicEnd.AddListener(OnBottomHatchCinematicEnd);
		}
		bool flag2 = !escapePod.topHatchUsed;
		topCinematicTarget.gameObject.SetActive(!flag2);
		topFirstUseCinematicTarget.gameObject.SetActive(flag2);
		if (flag2)
		{
			topFirstUseCinematicTarget.onCinematicStart.AddListener(OnTopHatchCinematicStart);
			topFirstUseCinematicTarget.onCinematicEnd.AddListener(OnTopHatchCinematicEnd);
		}
		initialized = true;
	}

	private void OnBottomHatchCinematicStart(CinematicModeEventData eventData)
	{
		rabbitray = SpawnCinematicCreature(rabbitrayPrefab, rabbitrayAttachPoint, "rabbit_ray_lifepod_cine");
		holefish1 = SpawnCinematicCreature(holefishPrefab, holefish1AttachPoint, "holefish_lifepod_cine");
		holefish2 = SpawnCinematicCreature(holefishPrefab, holefish2AttachPoint, "holefish01_lifepod_cine");
		escapePodRigidbody.isKinematic = true;
	}

	private void OnTopHatchCinematicStart(CinematicModeEventData eventData)
	{
		skyray = SpawnCinematicCreature(skyrayPrefab, skyrayAttachPoint, "bird_sml_lifepod_cine");
		if (skyray != null)
		{
			Roost component = skyray.GetComponent<Roost>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
		escapePodRigidbody.isKinematic = true;
	}

	private void OnBottomHatchCinematicEnd(CinematicModeEventData eventData)
	{
		ReleaseCreature(rabbitray);
		ReleaseCreature(holefish1);
		ReleaseCreature(holefish2);
		rabbitray = null;
		holefish1 = null;
		holefish2 = null;
		bottomFirstUseCinematicTarget.onCinematicStart.RemoveListener(OnBottomHatchCinematicStart);
		bottomFirstUseCinematicTarget.onCinematicEnd.RemoveListener(OnBottomHatchCinematicEnd);
		bottomCinematicTarget.gameObject.SetActive(value: true);
		bottomFirstUseCinematicTarget.gameObject.SetActive(value: false);
		escapePod.bottomHatchUsed = true;
		escapePodRigidbody.isKinematic = false;
	}

	private void OnTopHatchCinematicEnd(CinematicModeEventData eventData)
	{
		ReleaseCreature(skyray);
		skyray = null;
		topFirstUseCinematicTarget.onCinematicStart.RemoveListener(OnTopHatchCinematicStart);
		topFirstUseCinematicTarget.onCinematicEnd.RemoveListener(OnTopHatchCinematicEnd);
		topCinematicTarget.gameObject.SetActive(value: true);
		topFirstUseCinematicTarget.gameObject.SetActive(value: false);
		escapePod.topHatchUsed = true;
		escapePodRigidbody.isKinematic = false;
	}

	private void OnRabbitRayCinematicEnd()
	{
		ReleaseCreature(rabbitray);
		rabbitray = null;
	}

	private void OnHoleFishCinematicEnd()
	{
		ReleaseCreature(holefish1);
		holefish1 = null;
	}

	private void OnHoleFish01CinematicEnd()
	{
		ReleaseCreature(holefish2);
		holefish2 = null;
	}

	private void OnBirdSmlCinematicEnd()
	{
		ReleaseCreature(skyray);
		skyray = null;
	}

	private GameObject SpawnCinematicCreature(GameObject prefab, Transform attachPoint, string animationName)
	{
		GameObject obj = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
		obj.transform.SetParent(attachPoint, worldPositionStays: false);
		Creature component = obj.GetComponent<Creature>();
		if ((bool)component)
		{
			component.enabled = false;
			SafeAnimator.SetTrigger(component.GetAnimator(), animationName);
		}
		LargeWorldEntity component2 = obj.GetComponent<LargeWorldEntity>();
		if ((bool)component2 && (bool)LargeWorldStreamer.main)
		{
			LargeWorldStreamer.main.cellManager.UnregisterEntity(component2);
		}
		Rigidbody component3 = obj.GetComponent<Rigidbody>();
		if ((bool)component3)
		{
			component3.isKinematic = true;
		}
		return obj;
	}

	private void ReleaseCreature(GameObject creatureGO)
	{
		if (!(creatureGO == null))
		{
			creatureGO.transform.parent = null;
			Creature component = creatureGO.GetComponent<Creature>();
			if ((bool)component)
			{
				component.enabled = true;
			}
			Rigidbody component2 = component.GetComponent<Rigidbody>();
			if ((bool)component2)
			{
				component2.isKinematic = false;
			}
			LargeWorldEntity component3 = component.GetComponent<LargeWorldEntity>();
			if ((bool)component3 && (bool)LargeWorldStreamer.main)
			{
				LargeWorldStreamer.main.cellManager.RegisterEntity(component3);
			}
		}
	}
}
