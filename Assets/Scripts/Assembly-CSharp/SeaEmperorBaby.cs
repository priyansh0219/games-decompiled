using System.Collections;
using Gendarme;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
public class SeaEmperorBaby : Creature, IProtoTreeEventListener
{
	[AssertNotNull]
	public SeaEmperorBabyCinematicController cinematicController;

	[AssertNotNull]
	public SwimToTarget swimToTarget;

	[AssertNotNull]
	public DropEnzymes dropEnzymes;

	[AssertNotNull]
	public FMODAsset teleportSound;

	[AssertNotNull]
	public GameObject teleportEffectPrefab;

	[AssertNotNull]
	public Material teleportOverlayMaterial;

	public float overlayFXduration = 4f;

	public Vector3 teleporterPosition = new Vector3(243.4f, -1585.43f, -307.53f);

	public Vector3 motherInteractionScale = new Vector3(2.02f, 2.02f, 2.02f);

	public float scalingDuration = 20f;

	private int babyId = -1;

	private Coroutine adjustScale;

	private bool hatched;

	public void SetId(int id)
	{
		babyId = id;
	}

	public int GetId()
	{
		return babyId;
	}

	public bool IsAtTargetPosition(float range)
	{
		if (!swimToTarget.target)
		{
			return false;
		}
		return Vector3.Distance(base.transform.position, swimToTarget.target.position) < range;
	}

	public override void Start()
	{
		base.Start();
		SafeAnimator.SetBool(GetAnimator(), "hatched", value: true);
		hatched = true;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		SafeAnimator.SetBool(GetAnimator(), "hatched", hatched);
	}

	public void SwimToMother()
	{
		SeaEmperor main = SeaEmperor.main;
		if (!main)
		{
			Debug.LogErrorFormat(this, "SeaEmperor missing for baby {0}", babyId);
			return;
		}
		Transform babyAttachPoint = main.GetBabyAttachPoint(babyId);
		swimToTarget.SetTarget(babyAttachPoint);
		main.RegisterBaby(this);
		adjustScale = StartCoroutine(AdjustScale());
	}

	public void StopAdjustScale()
	{
		if (adjustScale != null)
		{
			StopCoroutine(adjustScale);
		}
	}

	private IEnumerator AdjustScale()
	{
		Vector3 initialScale = base.transform.localScale;
		float lerpFactor = 0f;
		while (lerpFactor < 1f)
		{
			base.transform.localScale = Vector3.Lerp(initialScale, motherInteractionScale, lerpFactor);
			lerpFactor += Time.deltaTime / scalingDuration;
			yield return null;
		}
	}

	public void SwimToTeleporter()
	{
		leashPosition = base.transform.position;
		dropEnzymes.enabled = true;
		swimToTarget.SetTarget(null);
		Invoke("SetTeleporterTarget", 5f * (float)babyId);
	}

	private void SetTeleporterTarget()
	{
		GameObject gameObject = new GameObject("TeleporterTarget");
		gameObject.transform.position = teleporterPosition;
		swimToTarget.SetTarget(gameObject.transform, 30f);
	}

	public void Teleport()
	{
		Utils.SpawnPrefabAt(teleportEffectPrefab, null, base.transform.position);
		ApplyAndForgetOverlayFX(base.gameObject);
		Utils.PlayFMODAsset(teleportSound, base.transform);
		Object.Destroy(base.gameObject);
	}

	private void ApplyAndForgetOverlayFX(GameObject targetObj)
	{
		targetObj.AddComponent<VFXOverlayMaterial>().ApplyAndForgetOverlay(teleportOverlayMaterial, "VFXOverlay: Warped", Color.clear, overlayFXduration);
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		Object.Destroy(base.gameObject);
	}

	public override void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		base.OnProtoDeserialize(serializer);
		if (base.transform.parent == null)
		{
			base.gameObject.SetActive(value: false);
			Object.Destroy(base.gameObject, 60f);
		}
	}
}
