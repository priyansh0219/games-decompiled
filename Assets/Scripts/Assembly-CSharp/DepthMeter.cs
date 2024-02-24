using UnityEngine;

public class DepthMeter : MonoBehaviour
{
	public MeshFilter safeZone;

	public MeshFilter weakZone;

	public MeshFilter crushZone;

	public TextMesh subLevel;

	public Transform subLevelLine;

	public float height = 2f;

	private CrushDamage crushDamage;

	private void Start()
	{
		SubRoot subRoot = Utils.FindAncestorWithComponent<SubRoot>(base.gameObject);
		crushDamage = subRoot.GetComponent<CrushDamage>();
	}

	private void LateUpdate()
	{
		float num = crushDamage.crushDepth * 1.5f;
		float num2 = 1f - crushDamage.crushDepth / num;
		Utils.Sandwich(crushZone, 0f, num2 * height);
		subLevel.text = crushDamage.GetDepth().ToString("0.0 ft");
		subLevelLine.localPosition = new Vector3(0f, (1f - crushDamage.GetDepth() / num) * height, 0.01f);
	}
}
