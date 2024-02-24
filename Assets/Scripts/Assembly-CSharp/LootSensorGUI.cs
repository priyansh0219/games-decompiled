using UWE;
using UnityEngine;

public class LootSensorGUI : MonoBehaviour
{
	public EmptySpaceClick emptySpaceClick;

	public GUIText statusText;

	private LootSensor lootSensor;

	public void Init(LootSensor sensor)
	{
		lootSensor = sensor;
	}

	private void OnEnable()
	{
		UWE.Utils.lockCursor = false;
		emptySpaceClick.gameObject.SetActive(value: true);
	}

	public void OnEmptySpaceClick()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		string lootSensingTechName = lootSensor.GetLootSensingTechName();
		string text = ((lootSensingTechName == "") ? "Place loot on sensor" : ("Scanning for " + lootSensingTechName));
		statusText.text = text;
	}

	private void OnDestroy()
	{
		emptySpaceClick.gameObject.SetActive(value: false);
		InputHandlerStack.main.Pop(base.gameObject);
	}
}
