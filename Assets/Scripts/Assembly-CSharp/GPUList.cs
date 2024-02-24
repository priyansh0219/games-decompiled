using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GPUList : MonoBehaviour
{
	public string workingDirectory;

	private Process process;

	private List<string> gpus = new List<string>();

	public IEnumerable<string> GetGPUs()
	{
		return gpus;
	}

	private void Start()
	{
		process = new Process();
		process.StartInfo.WorkingDirectory = workingDirectory;
		process.StartInfo.FileName = "SubnauticaGPUList.exe";
		process.StartInfo.Arguments = string.Empty;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.ErrorDialog = false;
		process.StartInfo.LoadUserProfile = false;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.RedirectStandardInput = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.ErrorDataReceived += OnMonitorError;
		process.OutputDataReceived += OnMonitorOutput;
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
	}

	private void OnDestroy()
	{
		if (process != null)
		{
			process.ErrorDataReceived -= OnMonitorError;
			process.OutputDataReceived -= OnMonitorOutput;
		}
	}

	private void OnMonitorError(object sendingProcess, DataReceivedEventArgs outLine)
	{
		UnityEngine.Debug.LogError("SubnauticaGPUList: " + outLine.Data, this);
	}

	private void OnMonitorOutput(object sendingProcess, DataReceivedEventArgs outLine)
	{
		gpus.Add(outLine.Data);
	}
}
