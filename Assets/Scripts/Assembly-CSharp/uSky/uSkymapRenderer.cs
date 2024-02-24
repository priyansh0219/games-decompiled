using UnityEngine;

namespace uSky
{
	[AddComponentMenu("uSky/uSkymap Renderer")]
	public class uSkymapRenderer : MonoBehaviour
	{
		public RenderTexture m_skyMap;

		public Material m_skymapMaterial;

		public Material m_oceanMaterial;

		public int SkymapResolution = 256;

		public bool DebugSkymap;

		private int m_frameCount;

		private uSkyManager m_uSM;

		protected uSkyManager uSM
		{
			get
			{
				if (m_uSM == null)
				{
					m_uSM = base.gameObject.GetComponent<uSkyManager>();
					if (m_uSM == null)
					{
						Debug.Log("Can't not find uSkyManager");
					}
				}
				return m_uSM;
			}
		}

		public RenderTexture GetSkyTexture()
		{
			return m_skyMap;
		}

		private void Start()
		{
			if (!SystemInfo.supportsRenderTextures)
			{
				Debug.LogWarning("RenderTexture is not supported with your Graphic Card");
				return;
			}
			if (uSM == null)
			{
				Debug.Log("Can NOT find uSkyManager, Please asign this uSkymapRenderer script to uSkyManager gameObject.");
				base.enabled = false;
				return;
			}
			RenderTextureFormat format = RenderTextureFormat.ARGB32;
			m_skyMap = new RenderTexture(SkymapResolution, SkymapResolution, 0, format);
			m_skyMap.filterMode = FilterMode.Trilinear;
			m_skyMap.wrapMode = TextureWrapMode.Clamp;
			m_skyMap.anisoLevel = 1;
			m_skyMap.useMipMap = true;
			m_skyMap.Create();
			m_skyMap.name = "SkyMap";
			if (m_skymapMaterial != null)
			{
				uSM.SetConstantMaterialProperties(m_skymapMaterial);
				uSM.SetVaryingMaterialProperties(m_skymapMaterial);
				Graphics.Blit(null, m_skyMap, m_skymapMaterial);
			}
		}

		private void Update()
		{
			if (m_skyMap != null && m_skymapMaterial != null)
			{
				if (m_frameCount == 1)
				{
					uSM.SetVaryingMaterialProperties(m_skymapMaterial);
				}
				m_frameCount++;
				Graphics.Blit(null, m_skyMap, m_skymapMaterial);
				uSM.SetVaryingMaterialProperties(m_skymapMaterial);
			}
		}

		private void OnDestroy()
		{
			m_skyMap.Release();
		}
	}
}
