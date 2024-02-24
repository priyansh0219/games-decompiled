using System;
using UWE;
using UnityEngine;

public class DiveReelNode : MonoBehaviour
{
	[AssertNotNull]
	public Transform arrow;

	[AssertNotNull]
	public Rigidbody rb;

	[AssertNotNull]
	public Transform firstNodeHolder;

	[AssertNotNull]
	public Transform standardNodeHolder;

	[AssertNotNull]
	public WorldForces worldForces;

	[AssertNotNull]
	public Light light;

	[AssertNotNull]
	public FMOD_CustomEmitter blinkSFX;

	[AssertNotNull]
	public Collider collider;

	[NonSerialized]
	public Transform previousArrowPos;

	public bool firstArrow;

	public float arrowDeployDelay = 2f;

	public float blinkDelay = 3f;

	[Header("Above Water Properties")]
	public float groundPushDistance = 3f;

	public float pushForce = 10f;

	public float forceDampening = 3f;

	private Transform useTransform;

	private bool arrowDeploy;

	private float arrowScale;

	private bool destroySelf;

	private float selfScale = 1f;

	private Material arrowMat;

	private Color baseColor;

	private bool blinking;

	private float lightStartIntensity;

	private float lastHitDistance;

	private bool _deployedOnSurface;

	private const float worldSettledPollFrequency = 3f;

	private bool isWorldSettled;

	private const float boundsSizeForHighSettings = 30f;

	private const float boundsSizeForLowSettings = 7f;

	private Vector3 boundsSize;

	private bool deployedOnSurface
	{
		get
		{
			return _deployedOnSurface;
		}
		set
		{
			if (_deployedOnSurface != value)
			{
				if (value)
				{
					InvokeRepeating("Raycast", 0f, 0.1f);
				}
				else
				{
					CancelInvoke();
				}
			}
			_deployedOnSurface = value;
			worldForces.aboveWaterOverride = _deployedOnSurface;
		}
	}

	private void Awake()
	{
		Player.main.playerController.IgnoreCollisions(collider, ignore: true);
	}

	private void Start()
	{
		useTransform = (firstArrow ? firstNodeHolder : arrow);
		Invoke("DeployArrow", arrowDeployDelay);
		Vector3 vector = ReturnDirectionToPrevious();
		if (vector != Vector3.zero)
		{
			arrow.rotation = Quaternion.LookRotation(vector);
		}
		useTransform.localScale = Vector3.zero;
		arrowMat = useTransform.GetComponentInChildren<MeshRenderer>().material;
		if ((bool)arrowMat)
		{
			baseColor = arrowMat.GetColor(ShaderPropertyID._Color);
		}
		firstNodeHolder.gameObject.SetActive(firstArrow);
		standardNodeHolder.gameObject.SetActive(!firstArrow);
		lightStartIntensity = light.intensity;
		deployedOnSurface = Player.main.forceWalkMotorMode || base.transform.position.y > 0f;
		float num = 30f;
		if (LargeWorldStreamer.main.streamerV2.GetActiveQualityLevel() == 0)
		{
			num = 7f;
		}
		boundsSize = new Vector3(num, num, num);
		InvokeRepeating("PollWorldSettled", 0f, 3f);
	}

	private void OnDestroy()
	{
		if (Player.main != null)
		{
			Player.main.playerController.IgnoreCollisions(collider, ignore: false);
		}
	}

	private void PollWorldSettled()
	{
		Bounds bb = new Bounds(base.transform.position, boundsSize);
		isWorldSettled = LargeWorldStreamer.main.IsRangeActiveAndBuilt(bb);
	}

	private void Raycast()
	{
		int layerMask = 1073741825;
		if (Physics.Raycast(base.transform.position, -Vector3.up, out var hitInfo, groundPushDistance, layerMask))
		{
			lastHitDistance = hitInfo.distance;
		}
		else
		{
			lastHitDistance = groundPushDistance;
		}
	}

	public void Blink(float delay)
	{
		Invoke("SetBlink", delay);
		Invoke("UnsetBlink", delay + blinkDelay);
	}

	private void SetBlink()
	{
		blinking = true;
	}

	private void UnsetBlink()
	{
		blinking = false;
	}

	private void DeployArrow()
	{
		arrowDeploy = true;
	}

	private void Update()
	{
		if (!arrowDeploy)
		{
			return;
		}
		float b = (blinking ? 2.5f : 1f);
		arrowScale = Mathf.Lerp(arrowScale, b, Time.deltaTime * 3f);
		useTransform.localScale = new Vector3(arrowScale, arrowScale, arrowScale);
		Quaternion b2 = Quaternion.identity;
		Vector3 vector = ReturnDirectionToPrevious();
		if (vector != Vector3.zero)
		{
			b2 = Quaternion.LookRotation(vector);
		}
		arrow.rotation = Quaternion.Slerp(arrow.rotation, b2, Time.deltaTime * 1.5f);
		if (destroySelf)
		{
			selfScale = Mathf.Lerp(selfScale, 0f, Time.deltaTime * 15f);
			base.transform.localScale = new Vector3(selfScale, selfScale, selfScale);
			light.intensity = lightStartIntensity * selfScale;
			if (selfScale < 0.01f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		Color color = arrowMat.GetColor(ShaderPropertyID._Color);
		if (blinking)
		{
			Color value = Color.Lerp(color, Color.green, Time.deltaTime * 3f);
			arrowMat.SetColor(ShaderPropertyID._Color, value);
		}
		else
		{
			Color value2 = Color.Lerp(color, baseColor, Time.deltaTime * 3f);
			arrowMat.SetColor(ShaderPropertyID._Color, value2);
		}
	}

	private void FixedUpdate()
	{
		if (deployedOnSurface)
		{
			bool flag = !isWorldSettled;
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, flag);
			if (!flag)
			{
				float num = Mathf.Max(pushForce / forceDampening, pushForce * Mathf.Clamp01(1f - lastHitDistance / groundPushDistance));
				rb.AddForceAtPosition(Vector3.up * num, base.transform.position);
			}
		}
	}

	private Vector3 ReturnDirectionToPrevious()
	{
		if (previousArrowPos == null || firstArrow)
		{
			return Vector3.zero;
		}
		return (previousArrowPos.position - arrow.position).normalized;
	}

	private void SetDestroySelf()
	{
		destroySelf = true;
	}

	public void DestroySelf(float delay)
	{
		Invoke("SetDestroySelf", delay);
	}
}
