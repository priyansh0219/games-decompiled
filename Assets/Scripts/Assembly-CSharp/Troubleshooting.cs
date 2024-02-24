using System.Text;
using TMPro;
using UnityEngine;

public class Troubleshooting : MonoBehaviour
{
	public TextMeshProUGUI systemSpecs;

	[AssertLocalization]
	private const string cpuKey = "CPU";

	[AssertLocalization]
	private const string ramKey = "RAM";

	[AssertLocalization]
	private const string gpuKey = "GPU";

	[AssertLocalization]
	private const string gpuRamKey = "GPURAM";

	[AssertLocalization]
	private const string osKey = "OS";

	[AssertLocalization]
	private const string threadsKey = "Threads";

	[AssertLocalization]
	private const string versionKey = "Version";

	[AssertLocalization]
	private const string buildDateKey = "BuildDate";

	[AssertLocalization]
	private const string megabytesKey = "megabytes";

	[AssertLocalization]
	private const string logicalProcessorsKey = "LogicalProcessors";

	private void Start()
	{
		updateSystemSpecs();
		Language.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		updateSystemSpecs();
	}

	private void updateSystemSpecs()
	{
		Language main = Language.main;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("<color=#FFA300FF>" + main.Get("CPU") + ": </color>{0}\n", SystemInfo.processorType);
		stringBuilder.AppendFormat("<color=#FFA300FF>" + main.Get("RAM") + ": </color>{0} " + main.Get("megabytes") + "\n", SystemInfo.systemMemorySize);
		stringBuilder.AppendFormat("<color=#FFA300FF>" + main.Get("GPU") + ": </color>{0}\n", SystemInfo.graphicsDeviceName);
		stringBuilder.AppendFormat("<color=#FFA300FF>" + main.Get("GPURAM") + ": </color>{0} " + main.Get("megabytes") + "\n", SystemInfo.graphicsMemorySize);
		stringBuilder.AppendFormat("<color=#FFA300FF>" + main.Get("OS") + ": </color>{0}\n", SystemInfo.operatingSystem);
		stringBuilder.AppendFormat("<color=#FFA300FF>" + main.Get("Threads") + ": </color>{0} " + main.Get("LogicalProcessors") + "\n", SystemInfo.processorCount);
		stringBuilder.AppendFormat("<color=#FFA300FF>" + main.Get("Version") + ": </color>{0}\n", SNUtils.GetPlasticChangeSetOfBuild());
		stringBuilder.AppendFormat("<color=#FFA300FF>" + main.Get("BuildDate") + ": </color>{0}\n", SNUtils.GetDateTimeOfBuild());
		systemSpecs.text = stringBuilder.ToString();
	}

	public void openTroubleshootingGuide()
	{
		PlatformUtils.OpenURL("https://unknownworlds.com/subnautica/steam-troubleshooting-guide/");
	}

	public void openBugReportingForums()
	{
		PlatformUtils.OpenURL("https://forums.unknownworlds.com/categories/subnautica-bug-reporting");
	}
}
