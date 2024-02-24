using UnityEngine;

namespace uSky
{
	[ExecuteInEditMode]
	[AddComponentMenu("uSky/uSky Fog (Image Effects)")]
	public class uSkyFog : MonoBehaviour
	{
		public enum FogModes
		{
			Linear = 1,
			Exponential = 2,
			Exponential_Squared = 3
		}

		public FogModes fogMode = FogModes.Exponential;

		public bool useRadialDistance;

		[Range(0.0001f, 1f)]
		public float Density = 0.001f;

		[Range(0.06f, 0.4f)]
		public float ColorDecay = 0.2f;

		[Range(0f, 1f)]
		public float Scattering = 1f;

		[Range(0f, 0.1f)]
		public float HorizonOffset;

		public float StartDistance;

		public float EndDistance = 300f;

		public Material FogMaterial;

		private uSkyManager _uSM;

		private Material _skybox;

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

		private Material skybox
		{
			get
			{
				if (_skybox == null && uSM != null)
				{
					_skybox = base.gameObject.GetComponent<uSkyManager>().SkyboxMaterial;
				}
				return _skybox;
			}
		}

		private void Start()
		{
			uSM.SetConstantMaterialProperties(FogMaterial);
			updateFog(FogMaterial);
			skybox.SetFloat(ShaderPropertyID._HorizonOffset, HorizonOffset);
		}

		private void Update()
		{
			FogModes fogModes = fogMode;
			float density = Density;
			float startDistance = StartDistance;
			float endDistance = EndDistance;
			bool flag = fogModes == FogModes.Linear;
			float num = (flag ? (endDistance - startDistance) : 0f);
			float num2 = ((Mathf.Abs(num) > 0.0001f) ? (1f / num) : 0f);
			Vector4 value = default(Vector4);
			value.x = density * 1.2011224f;
			value.y = density * 1.442695f;
			value.z = (flag ? (0f - num2) : 0f);
			value.w = (flag ? (endDistance * num2) : 0f);
			FogMaterial.SetVector(ShaderPropertyID._SceneFogParams, value);
			FogMaterial.SetVector(ShaderPropertyID._SceneFogMode, new Vector4((float)fogModes, useRadialDistance ? 1 : 0, 0f, 0f));
			FogMaterial.SetVector(ShaderPropertyID._fParams, new Vector4((fogMode == FogModes.Linear) ? (0f - Mathf.Max(StartDistance, 0f)) : 0f, ColorDecay, Scattering, 0f));
			updateFog(FogMaterial);
		}

		private void updateFog(Material mat)
		{
			if (uSM != null && mat != null && uSM.SkyUpdate)
			{
				uSM.SetVaryingMaterialProperties(mat);
				skybox.SetFloat(ShaderPropertyID._HorizonOffset, HorizonOffset);
			}
		}

		public void SetFogDensity(float value)
		{
			Density = value;
		}

		public void SetFogColorDecay(float value)
		{
			ColorDecay = value;
		}
	}
}
