using UWE;
using UnityEngine;

public class CrushDamage : MonoBehaviour
{
	[AssertNotNull]
	public LiveMixin liveMixin;

	public Vehicle vehicle;

	public float kBaseCrushDepth = 100f;

	public float damagePerCrush = 20f;

	public float crushPeriod = 3f;

	public VoiceNotification crushDepthUpdate;

	public FMOD_CustomEmitter soundOnDamage;

	private LazyFrameValue<float> depthCache;

	[AssertLocalization(1)]
	private const string crushDepthNowFormatKey = "CrushDepthNow";

	public float extraCrushDepth { get; private set; }

	public float crushDepth { get; private set; }

	public float GetDepth()
	{
		if (depthCache == null)
		{
			return 0f;
		}
		return depthCache.Get();
	}

	private void Awake()
	{
		depthCache = new LazyFrameValue<float>(() => Ocean.GetDepthOf(base.gameObject));
		crushDepth = kBaseCrushDepth;
	}

	private void Start()
	{
		InvokeRepeating("CrushDamageUpdate", Random.value, crushPeriod);
	}

	public void SetExtraCrushDepth(float depth)
	{
		extraCrushDepth = depth;
		UpdateDepthClassification();
	}

	public Ocean.DepthClass GetDepthClass()
	{
		Ocean.DepthClass result = Ocean.DepthClass.Surface;
		float depth = GetDepth();
		if (depth > crushDepth)
		{
			result = Ocean.DepthClass.Crush;
		}
		else if (depth > 0.1f)
		{
			result = Ocean.DepthClass.Safe;
		}
		return result;
	}

	private void UpdateDepthClassification()
	{
		if (base.gameObject.activeInHierarchy)
		{
			float a = crushDepth;
			crushDepth = kBaseCrushDepth + extraCrushDepth;
			if (!Mathf.Approximately(a, crushDepth) && !WaitScreen.IsWaiting)
			{
				ErrorMessage.AddMessage(Language.main.GetFormat("CrushDepthNow", crushDepth));
			}
		}
	}

	public bool GetCanTakeCrushDamage()
	{
		if (!GameModeUtils.RequiresReinforcements())
		{
			return false;
		}
		if (!vehicle)
		{
			return true;
		}
		if (!vehicle.GetRecentlyUndocked() && !vehicle.docked && !vehicle.precursorOutOfWater)
		{
			return !vehicle.IsInsideAquarium();
		}
		return false;
	}

	private void CrushDamageUpdate()
	{
		if (base.gameObject.activeInHierarchy && base.enabled && GetCanTakeCrushDamage() && GetDepth() > crushDepth)
		{
			liveMixin.TakeDamage(damagePerCrush, base.transform.position, DamageType.Pressure);
			if ((bool)soundOnDamage)
			{
				soundOnDamage.Play();
			}
		}
	}
}
