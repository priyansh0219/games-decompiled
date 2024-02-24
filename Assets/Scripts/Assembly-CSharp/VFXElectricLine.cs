using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VFXElectricLine : MonoBehaviour
{
	public Vector3 origin;

	public Vector3 target;

	public Vector3 originVector;

	public float originForce;

	public bool isLocal;

	public int segments = 10;

	public Vector3 noiseAmplitude = new Vector3(1f, 1f, 1f);

	public Vector3 noiseFrequency = new Vector3(1f, 1f, 1f);

	public Vector3 noiseSpeed = new Vector3(1f, 1f, 1f);

	public bool randomOffset;

	private LineRenderer line;

	public Vector3 offset;

	private bool lineIsEnabled;

	private void Awake()
	{
		line = base.transform.GetComponent<LineRenderer>();
		line.enabled = false;
		line.SetVertexCount(segments);
		if (isLocal)
		{
			origin = new Vector3(0f, 0f, 0f);
		}
		if (randomOffset)
		{
			offset = new Vector3(Random.value, Random.value, Random.value);
		}
		if (offset != Vector3.zero)
		{
			Material material = base.gameObject.GetComponent<Renderer>().material;
			Vector2 mainTextureOffset = material.mainTextureOffset;
			GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offset.x, mainTextureOffset.y);
			mainTextureOffset = material.GetTextureOffset(ShaderPropertyID._DeformMap);
			GetComponent<Renderer>().material.SetTextureOffset(ShaderPropertyID._DeformMap, new Vector2(offset.y, mainTextureOffset.y));
		}
	}

	private void Update()
	{
		float num = Vector3.Distance(origin, target);
		for (int i = 0; i < segments; i++)
		{
			float num2 = (float)i / ((float)segments - 1f);
			Vector3 position = ((originForce == 0f) ? Vector3.Lerp(origin, target, num2) : Vector3.Lerp(origin + originVector * num * num2, target, Mathf.Pow(num2, originForce)));
			float num3 = 1f - Mathf.Abs(num2 * 2f - 1f);
			position.x += Mathf.Sin(num2 * noiseFrequency.x + (offset.x + Time.time) * noiseSpeed.x) * noiseAmplitude.x * num3;
			position.y += Mathf.Sin(num2 * noiseFrequency.y + (offset.y + Time.time) * noiseSpeed.y) * noiseAmplitude.y * num3;
			position.z += Mathf.Sin(num2 * noiseFrequency.z + (offset.z + Time.time) * noiseSpeed.z) * noiseAmplitude.z * num3;
			line.SetPosition(i, position);
		}
		if (!lineIsEnabled)
		{
			line.enabled = true;
			lineIsEnabled = true;
		}
	}

	private void OnDisable()
	{
		line.enabled = false;
		lineIsEnabled = false;
	}

	private void OnDestroy()
	{
		if (randomOffset)
		{
			Object.DestroyImmediate(GetComponent<Renderer>().material);
		}
	}
}
