using UnityEngine;

public class VFXShockerElec : MonoBehaviour
{
	public GameObject[] elecLinesGO;

	public Transform[] elecPoints;

	public VFXShockerElecLine ringElecLine;

	public float ringDuration = 0.05f;

	public GameObject elecLight;

	private int ringIndex;

	private float ringTimer;

	private float delayTimer;

	private bool waitForNextRing;

	public Utils.MonitoredValue<bool> makesRings = new Utils.MonitoredValue<bool>();

	private Shocker shocker;

	private void Start()
	{
		shocker = base.gameObject.GetComponent<Shocker>();
		ringElecLine.elecPoints = new Transform[5];
		SetRingElecPoints();
		makesRings.changedEvent.AddHandler(this, OnMakesRingChanged);
		makesRings.Update(newValue: true);
	}

	private void SetRingElecPoints()
	{
		if (ringIndex >= elecPoints.Length)
		{
			waitForNextRing = true;
			delayTimer = Random.Range(0.8f, 1.3f);
			ringIndex = 0;
			elecLight.SetActive(value: false);
			ringElecLine.gameObject.SetActive(value: false);
			return;
		}
		if (ringIndex == 0)
		{
			waitForNextRing = false;
			elecLight.SetActive(value: true);
			ringElecLine.gameObject.SetActive(value: true);
		}
		ringElecLine.elecPoints[0] = elecPoints[ringIndex];
		ringElecLine.elecPoints[1] = elecPoints[ringIndex + 1];
		ringElecLine.elecPoints[2] = elecPoints[ringIndex + 2];
		ringElecLine.elecPoints[3] = elecPoints[ringIndex + 3];
		ringElecLine.elecPoints[4] = elecPoints[ringIndex];
		ringIndex += 4;
		ringTimer = 0f;
	}

	private void Update()
	{
		if (shocker.Aggression.Value > 0.3f)
		{
			makesRings.Update(newValue: false);
		}
		else
		{
			makesRings.Update(newValue: true);
		}
		for (int i = 0; i < elecLinesGO.Length; i++)
		{
			VFXShockerElecLine component = elecLinesGO[i].GetComponent<VFXShockerElecLine>();
			if (component != null)
			{
				component.scaleFactor.Update(base.transform.localScale.x);
			}
		}
		ringElecLine.scaleFactor.Update(base.transform.localScale.x);
	}

	private void LateUpdate()
	{
		if (makesRings.value)
		{
			ringTimer += Time.deltaTime;
			delayTimer -= Time.deltaTime;
			if (delayTimer < 0f)
			{
				if (ringTimer > ringDuration * GetRingDurationMult())
				{
					SetRingElecPoints();
				}
				Vector3 position = ringElecLine.elecPoints[Random.Range(0, ringElecLine.elecPoints.Length)].position;
				FlashingLightHelpers.SafePositionChangePreFrame(elecLight.transform, position);
			}
		}
		else
		{
			Vector3 position2 = elecPoints[Random.Range(0, elecPoints.Length)].position;
			FlashingLightHelpers.SafePositionChangePreFrame(elecLight.transform, position2);
		}
	}

	private void OnMakesRingChanged(Utils.MonitoredValue<bool> makesRings)
	{
		for (int i = 0; i < elecLinesGO.Length; i++)
		{
			elecLinesGO[i].SetActive(!makesRings.value);
		}
		ringElecLine.gameObject.SetActive(makesRings.value);
		if (!makesRings.value)
		{
			elecLight.SetActive(value: true);
		}
	}

	private void ToggleFX(bool enableBool)
	{
		if (makesRings.value)
		{
			ringElecLine.gameObject.SetActive(enableBool);
			return;
		}
		for (int i = 0; i < elecLinesGO.Length; i++)
		{
			elecLinesGO[i].SetActive(enableBool);
		}
	}

	private void OnDisable()
	{
		ToggleFX(enableBool: false);
	}

	private void OnEnable()
	{
		ToggleFX(enableBool: true);
	}

	private void OnKill()
	{
		base.enabled = false;
	}

	private float GetRingDurationMult()
	{
		if (!MiscSettings.flashes)
		{
			return 10f;
		}
		return 1f;
	}
}
