using Gendarme;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
public class CyclopsHolographicHUD_WarningPings : MonoBehaviour
{
	public enum WarningTypes
	{
		Damage = 0,
		Fire = 1,
		LavaLarva = 2
	}

	public WarningTypes warningType;

	[AssertNotNull]
	public GameObject warningPing;

	[AssertNotNull]
	public Animator animator;

	public GameObject damageText;

	public GameObject labelDot;

	public LineRenderer lineRenderer;

	private CyclopsHolographicHUD hudParent;

	private bool isDespawning;

	private Vector3 initialScale;

	private Vector3[] linePositions = new Vector3[2];

	private BehaviourLOD LOD;

	private void Start()
	{
		initialScale = warningPing.transform.localScale;
		hudParent = base.transform.GetComponentInParent<CyclopsHolographicHUD>();
		LOD = hudParent.LOD;
		if (warningType == WarningTypes.Damage && !(damageText == null) && !(labelDot == null) && !(lineRenderer == null))
		{
			Vector3 vector = Vector3.Normalize(base.transform.position - hudParent.transform.position);
			Vector3 position = base.transform.position + vector * 0.35f;
			labelDot.transform.position = position;
			linePositions[0] = base.transform.position;
			linePositions[1] = labelDot.transform.position;
			lineRenderer.SetPositions(linePositions);
			Vector3 vector2 = new Vector3(0f, 0f, 58f);
			if (labelDot.transform.localPosition.z < base.transform.localPosition.z)
			{
				vector2 *= -1f;
			}
			damageText.transform.localPosition = labelDot.transform.localPosition + vector2;
		}
	}

	private void Update()
	{
		if (LOD.IsFull())
		{
			warningPing.transform.LookAt(MainCamera.camera.transform.position);
			if (damageText != null && warningType == WarningTypes.Damage)
			{
				labelDot.transform.LookAt(MainCamera.camera.transform.position);
				labelDot.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
				damageText.transform.LookAt(MainCamera.camera.transform.position);
				damageText.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
				linePositions[0] = base.transform.position;
				linePositions[1] = labelDot.transform.position;
				lineRenderer.SetPositions(linePositions);
			}
		}
	}

	private void Despawn()
	{
		Object.Destroy(base.gameObject);
	}

	public void DespawnIcon()
	{
		if (!isDespawning)
		{
			isDespawning = true;
			animator.SetTrigger("Death");
			Invoke("Despawn", 3f);
		}
	}
}
