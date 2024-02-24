using UnityEngine;

namespace uSky
{
	[AddComponentMenu("uSky/Play TOD")]
	[RequireComponent(typeof(uSkyManager))]
	public class PlayTOD : MonoBehaviour
	{
		public bool PlayTimelapse = true;

		public float PlaySpeed = 0.1f;

		private uSkyManager m_uSM;

		private uSkyManager uSM
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

		private void Start()
		{
			if (PlayTimelapse)
			{
				uSM.SkyUpdate = true;
			}
		}

		private void Update()
		{
			if (PlayTimelapse)
			{
				uSM.Timeline += Time.deltaTime * PlaySpeed;
			}
		}
	}
}
