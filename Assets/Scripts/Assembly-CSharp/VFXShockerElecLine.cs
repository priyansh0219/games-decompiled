using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VFXShockerElecLine : MonoBehaviour
{
	public Transform[] elecPoints;

	public int segments = 4;

	public Vector3 noiseAmplitude = new Vector3(1f, 1f, 1f);

	public Vector3 noiseFrequency = new Vector3(1f, 1f, 1f);

	public Vector3 noiseSpeed = new Vector3(1f, 1f, 1f);

	public bool randomOffset;

	private LineRenderer line;

	public Vector3 offset;

	private bool lineIsEnabled;

	public int vCount;

	public float lineWidth = 0.7f;

	public Utils.MonitoredValue<float> scaleFactor = new Utils.MonitoredValue<float>();

	private Vector3 initialNoiseAmplitude;

	private FlashingLightHelpers.ShaderVector4ScalerToken textureSpeedToken;

	private Vector3 currentNoiseSpeed;

	private void Awake()
	{
		line = base.transform.GetComponent<LineRenderer>();
		vCount = (elecPoints.Length - 1) * segments + 1;
		line.SetVertexCount(vCount);
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
		initialNoiseAmplitude = noiseAmplitude;
		scaleFactor.Update(1f);
		scaleFactor.changedEvent.AddHandler(this, OnScaleFactorChanged);
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		Material material2 = line.material;
		textureSpeedToken = FlashingLightHelpers.CreateUberShaderVector4ScalerToken(material2);
		currentNoiseSpeed = noiseSpeed;
		UpdateSpeed();
	}

	private void OnScaleFactorChanged(Utils.MonitoredValue<float> scaleFactor)
	{
		float num = lineWidth * scaleFactor.value;
		line.SetWidth(num, num);
		noiseAmplitude = initialNoiseAmplitude * scaleFactor.value;
	}

	private void LateUpdate()
	{
		int num = 0;
		for (int i = 0; i < vCount; i++)
		{
			Vector3 position;
			if (i % segments == 0)
			{
				num = Mathf.RoundToInt(i / segments);
				position = elecPoints[num].position;
			}
			else
			{
				float num2 = (float)i / (float)segments - (float)num;
				position = Vector3.Lerp(elecPoints[num].position, elecPoints[num + 1].position, num2);
				float num3 = 1f - Mathf.Abs(num2 * 2f - 1f);
				position.x += Mathf.Sin(num2 * noiseFrequency.x + (offset.x + Time.time) * currentNoiseSpeed.x) * noiseAmplitude.x * num3;
				float num4 = num2 * noiseFrequency.y + (float)i / (float)vCount + (offset.y + Time.time) * currentNoiseSpeed.y;
				position.y += (num4 - (float)(int)num4) * noiseAmplitude.y * num3;
				position.z += Mathf.Sin(num2 * noiseFrequency.z + (offset.z + Time.time) * currentNoiseSpeed.z) * noiseAmplitude.z * num3;
			}
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
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void OnFlashesEnabled(Utils.MonitoredValue<bool> isFlashesEnabled)
	{
		UpdateSpeed();
	}

	private void UpdateSpeed()
	{
		if (MiscSettings.flashes)
		{
			textureSpeedToken.RestoreScale();
			currentNoiseSpeed = noiseSpeed;
		}
		else
		{
			textureSpeedToken.SetScale(0.01f);
			currentNoiseSpeed = noiseSpeed * 0.01f;
		}
	}
}
