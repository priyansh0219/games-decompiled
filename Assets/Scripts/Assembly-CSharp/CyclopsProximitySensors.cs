using UnityEngine;

public class CyclopsProximitySensors : MonoBehaviour
{
	private class SensorCastData
	{
		public float radius { get; private set; }

		public float distance { get; private set; }

		public SensorCastData(float radius, float distance)
		{
			this.radius = radius;
			this.distance = distance;
		}
	}

	public Animator uiWarningPanel;

	public GameObject uiWarningIcon;

	public FMOD_CustomEmitter proximitySound;

	[AssertNotNull(AssertNotNullAttribute.Options.IgnorePrefabs)]
	public CyclopsMotorMode motorMode;

	[AssertNotNull]
	public GameObject[] uiWarningDot;

	[AssertNotNull(AssertNotNullAttribute.Options.IgnorePrefabs)]
	public GameObject[] sensor;

	private readonly SensorCastData[] sensorCastData = new SensorCastData[8]
	{
		new SensorCastData(5f, 5f),
		new SensorCastData(6f, 4f),
		new SensorCastData(6f, 4f),
		new SensorCastData(12f, 3f),
		new SensorCastData(8.5f, 7.5f),
		new SensorCastData(10f, 7.5f),
		new SensorCastData(5f, 5f),
		new SensorCastData(5f, 5f)
	};

	private float pingSoundInterval = 1f;

	private float sensorTime = 1f;

	private const int totalSensors = 8;

	private float NOCOLLISION = -1f;

	private bool detectedCollision;

	private bool panelActive;

	private float[] returnDistances = new float[8];

	private void Start()
	{
		Player.main.playerModeChanged.AddHandler(base.gameObject, OnPlayerModeChange);
		for (int i = 0; i < 8; i++)
		{
			uiWarningDot[i].SetActive(value: false);
		}
	}

	public void OnPlayerModeChange(Player.Mode mode)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (mode == Player.Mode.Piloting)
		{
			Invoke("DetectCollision", sensorTime);
			return;
		}
		CancelInvoke();
		if (uiWarningPanel.GetBool("PanelActive"))
		{
			uiWarningPanel.SetBool("PanelActive", value: false);
			uiWarningIcon.SetActive(value: false);
			for (int i = 0; i < 8; i++)
			{
				uiWarningDot[i].SetActive(value: false);
			}
		}
	}

	private void PingSound()
	{
		proximitySound.Play();
		Invoke("PingSound", pingSoundInterval);
	}

	private void DetectCollision()
	{
		detectedCollision = false;
		bool flag = false;
		float num = NOCOLLISION;
		float num2 = 15f;
		if (motorMode.engineOn)
		{
			for (int i = 0; i < 8; i++)
			{
				returnDistances[i] = NOCOLLISION;
				float distance = sensorCastData[i].distance;
				float radius = sensorCastData[i].radius;
				Vector3 position = sensor[i].transform.position;
				Vector3 forward = sensor[i].transform.forward;
				int layerMask = 1073741824;
				if (Physics.SphereCast(position, radius, forward, out var hitInfo, distance, layerMask))
				{
					detectedCollision = true;
					returnDistances[i] = hitInfo.distance;
					if (hitInfo.distance < distance / 4f)
					{
						flag = true;
					}
					if (hitInfo.distance < num || num == NOCOLLISION)
					{
						num2 = distance;
						num = hitInfo.distance;
					}
				}
			}
		}
		if (num != NOCOLLISION)
		{
			pingSoundInterval = num / num2 + 0.2f;
			if (!IsInvoking("PingSound"))
			{
				Invoke("PingSound", pingSoundInterval);
			}
		}
		else
		{
			proximitySound.Stop();
			CancelInvoke("PingSound");
		}
		float time = sensorTime;
		if (detectedCollision && motorMode.engineOn)
		{
			if (!uiWarningPanel.GetBool("PanelActive"))
			{
				uiWarningPanel.SetBool("PanelActive", value: true);
			}
			time = 0.25f;
		}
		else if (uiWarningPanel.GetBool("PanelActive"))
		{
			uiWarningPanel.SetBool("PanelActive", value: false);
		}
		if (flag && motorMode.engineOn)
		{
			uiWarningIcon.SetActive(value: true);
		}
		else
		{
			uiWarningIcon.SetActive(value: false);
		}
		for (int j = 0; j < 8; j++)
		{
			if (returnDistances[j] != NOCOLLISION && motorMode.engineOn)
			{
				if (!uiWarningDot[j].activeInHierarchy)
				{
					uiWarningDot[j].SetActive(value: true);
				}
			}
			else
			{
				uiWarningDot[j].SetActive(value: false);
			}
		}
		Invoke("DetectCollision", time);
	}
}
