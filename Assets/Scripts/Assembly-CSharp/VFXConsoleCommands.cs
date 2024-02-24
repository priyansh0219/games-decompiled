using UnityEngine;

public class VFXConsoleCommands : MonoBehaviour
{
	private bool overdrawEnabled;

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "vfx");
		DevConsole.RegisterConsoleCommand(this, "overdraw");
	}

	private void OnConsoleCommand_vfx(NotificationCenter.Notification n)
	{
		bool flag = ((string)n.data[0]).Contains("cyclopssmoke");
		if (n.data.Count > 1 && DevConsole.ParseFloat(n, 1, out var value) && flag)
		{
			UpdateCyclopsSmokeScreenFX(value);
		}
	}

	private void OnConsoleCommand_overdraw()
	{
		overdrawEnabled = !overdrawEnabled;
		if (overdrawEnabled)
		{
			Shader shader = Shader.Find("Debug/Show Overdraw");
			MainCamera.camera.SetReplacementShader(shader, "");
		}
		else
		{
			MainCamera.camera.ResetReplacementShader();
		}
	}

	private void UpdateCyclopsSmokeScreenFX(float intensityScalar)
	{
		intensityScalar = Mathf.Clamp(intensityScalar, 0f, 1f);
		CyclopsSmokeScreenFXController component = MainCamera.camera.GetComponent<CyclopsSmokeScreenFXController>();
		if (component != null)
		{
			component.intensity = intensityScalar;
			ErrorMessage.AddDebug("Setting CyclopsSmokeScreenFXController to " + intensityScalar + ".");
		}
	}
}
