using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Base))]
public class BaseHullStrength : MonoBehaviour, IProtoEventListener
{
	private const float InitialStrength = 10f;

	private const float CrushPeriod = 2f;

	private const float damagePerCrush = 10f;

	[AssertLocalization(2)]
	private const string hullStrengthChangedMessage = "BaseHullStrChanged";

	[AssertLocalization(1)]
	private const string hullStrengthDamagedMessage = "BaseHullStrDamageDetected";

	public FMODAsset[] crushSounds;

	private Base baseComp;

	private float totalStrength = 10f;

	private List<LiveMixin> victims = new List<LiveMixin>();

	private void Awake()
	{
		baseComp = GetComponent<Base>();
		baseComp.onPostRebuildGeometry += OnPostRebuildGeometry;
	}

	private void OnDestroy()
	{
		if ((bool)baseComp)
		{
			baseComp.onPostRebuildGeometry -= OnPostRebuildGeometry;
		}
	}

	private void Start()
	{
		InvokeRepeating("CrushDamageUpdate", Random.value, 2f);
	}

	private void OnPostRebuildGeometry(Base b)
	{
		if (!GameModeUtils.RequiresReinforcements())
		{
			return;
		}
		float num = 10f;
		victims.Clear();
		foreach (Int3 allCell in baseComp.AllCells)
		{
			if (baseComp.GridToWorld(allCell).y < 0f)
			{
				Transform cellObject = baseComp.GetCellObject(allCell);
				if (cellObject != null)
				{
					victims.Add(cellObject.GetComponent<LiveMixin>());
					num += baseComp.GetHullStrength(allCell);
				}
			}
		}
		if (!Mathf.Approximately(num, totalStrength) && !WaitScreen.IsWaiting)
		{
			ErrorMessage.AddMessage(Language.main.GetFormat("BaseHullStrChanged", num - totalStrength, num));
		}
		totalStrength = num;
	}

	public float GetStructualIntegrity()
	{
		if (GameModeUtils.RequiresReinforcements())
		{
			return totalStrength;
		}
		return 10f;
	}

	private void CrushDamageUpdate()
	{
		if (GameModeUtils.RequiresReinforcements() && totalStrength < 0f && victims.Count > 0)
		{
			LiveMixin random = victims.GetRandom();
			random.TakeDamage(10f, random.transform.position, DamageType.Pressure);
			int num = 0;
			if (totalStrength <= -3f)
			{
				num = 2;
			}
			else if (totalStrength <= -2f)
			{
				num = 1;
			}
			if (crushSounds[num] != null)
			{
				Utils.PlayFMODAsset(crushSounds[num], random.transform);
			}
			ErrorMessage.AddMessage(Language.main.GetFormat("BaseHullStrDamageDetected", totalStrength));
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}

	public float GetTotalStrength()
	{
		return totalStrength;
	}
}
