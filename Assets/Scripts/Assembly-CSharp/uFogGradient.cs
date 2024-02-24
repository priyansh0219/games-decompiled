using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("uSky/uFog Gradient (Stardard Fog)")]
public class uFogGradient : MonoBehaviour
{
	public Gradient FogColor = new Gradient
	{
		colorKeys = new GradientColorKey[6]
		{
			new GradientColorKey(new Color32(19, 32, 45, byte.MaxValue), 0.22f),
			new GradientColorKey(new Color32(189, 148, 62, byte.MaxValue), 0.25f),
			new GradientColorKey(new Color32(223, 246, 252, byte.MaxValue), 0.27f),
			new GradientColorKey(new Color32(223, 246, 252, byte.MaxValue), 0.73f),
			new GradientColorKey(new Color32(189, 148, 62, byte.MaxValue), 0.75f),
			new GradientColorKey(new Color32(19, 32, 45, byte.MaxValue), 0.78f)
		},
		alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		}
	};

	private uSkyManager _uSM;

	private uSkyManager uSM
	{
		get
		{
			if (_uSM == null)
			{
				_uSM = base.gameObject.GetComponent<uSkyManager>();
			}
			return _uSM;
		}
	}

	private void Start()
	{
		RenderSettings.fogColor = currentFogColor();
	}

	private void Update()
	{
		if (uSM != null && uSM.SkyUpdate)
		{
			RenderSettings.fogColor = currentFogColor();
		}
	}

	private Color currentFogColor()
	{
		float time = ((uSM != null) ? uSM.Timeline01 : 1f);
		return FogColor.Evaluate(time);
	}
}
