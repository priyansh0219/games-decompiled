using UnityEngine;
using UnityEngine.UI;

public class MainMenuFOV : MonoBehaviour
{
	public Text currentValueLabel;

	public Slider slider;

	private void Start()
	{
		currentValueLabel.text = MainCamera.camera.fieldOfView.ToString();
		slider.value = MainCamera.camera.fieldOfView;
	}

	public void UpdateFOV(float value)
	{
		currentValueLabel.text = value.ToString();
	}
}
