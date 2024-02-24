using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class MinSpecWarning : MonoBehaviour
{
	private enum GPUClass
	{
		Unknown = 0,
		Black = 1,
		White = 2
	}

	private const int minSysRamMBs = 3584;

	private const string urlSupportForums = "http://steamcommunity.com/app/264710/discussions/";

	public bool debug;

	[AssertNotNull]
	public GameObject mainMenu;

	[AssertNotNull]
	public GameObject[] panels;

	[AssertNotNull]
	public GameObject ramOK;

	[AssertNotNull]
	public GameObject ramNOK;

	[AssertNotNull]
	public GameObject cpuOK;

	[AssertNotNull]
	public GameObject cpuNOK;

	[AssertNotNull]
	public GameObject gpuOK;

	[AssertNotNull]
	public GameObject gpuNOK;

	[AssertNotNull]
	public GameObject gpuUNK;

	[AssertNotNull]
	public TextMeshProUGUI cpuDetected;

	[AssertNotNull]
	public TextMeshProUGUI cpuL;

	[AssertNotNull]
	public TextMeshProUGUI gpuDetected;

	[AssertNotNull]
	public TextMeshProUGUI ramDetected;

	private readonly HashSet<string> whiteList = new HashSet<string>
	{
		"AMD Mobility Radeon HD 5800 Series", "AMD Radeon (TM) R7 360 Series", "AMD Radeon (TM) R7 370 Series Graphics", "AMD Radeon (TM) R7 370 Series", "AMD Radeon (TM) R9 200 Series", "AMD Radeon (TM) R9 360", "AMD Radeon (TM) R9 380 Series", "AMD Radeon (TM) R9 390 Series", "AMD Radeon (TM) R9 Fury Series", "AMD Radeon (TM) RX 480",
		"AMD Radeon HD 5670", "AMD Radeon HD 5800 Series", "AMD Radeon HD 6700 Series", "AMD Radeon HD 6800 Series", "AMD Radeon HD 6900 Series", "AMD Radeon HD 6900M Series", "AMD Radeon HD 6970M OpenGL Engine", "AMD Radeon HD 7000 series", "AMD Radeon HD 7500G", "AMD Radeon HD 7660D",
		"AMD Radeon HD 7660G", "AMD Radeon HD 7670M", "AMD Radeon HD 7700 Series", "AMD Radeon HD 7800 Series", "AMD Radeon HD 7900 Series", "AMD Radeon HD 7970M", "AMD Radeon HD 8570", "AMD Radeon HD 8570D", "AMD Radeon HD 8670D", "AMD Radeon Pro 560 OpenGL Engine",
		"AMD Radeon R7 200 Series", "AMD Radeon R7 240", "AMD Radeon R7 250 Series", "AMD Radeon R9 200 / HD 7900 Series", "AMD Radeon R9 200 Series", "AMD Radeon R9 255", "AMD Radeon R9 M265X", "AMD Radeon R9 M290X OpenGL Engine", "AMD Radeon R9 M295X OpenGL Engine", "AMD Radeon R9 M370X OpenGL Engine",
		"AMD Radeon R9 M380 OpenGL Engine", "AMD Radeon R9 M390 OpenGL Engine", "AMD Radeon R9 M395 OpenGL Engine", "AMD Radeon R9 M395X OpenGL Engine", "AMD Radeon(TM) R6 Graphics", "AMD Radeon(TM) R9 270", "AMD Radeon(TM) Vega 8 Graphics", "ASUS HD7770 Series", "ASUS R7 265 Series", "ASUS R9 270X Series",
		"ASUS R9 280 Series", "ATI Radeon HD 4850 OpenGL Engine", "ATI Radeon HD 5670 OpenGL Engine", "ATI Radeon HD 5670", "ATI Radeon HD 5700 Series", "ATI Radeon HD 5750 OpenGL Engine", "ATI Radeon HD 5770 OpenGL Engine", "ATI Radeon HD 5800 Series", "Intel(R) HD Graphics 4600", "Intel(R) HD Graphics 530",
		"Intel(R) HD Graphics 630", "Intel(R) Iris(TM) Graphics 540", "Intel(R) Iris(TM) Graphics 550", "Intel(R) Iris(TM) Graphics 6100", "Intel(R) Iris(TM) Graphics 650", "Intel(R) Iris(TM) Plus Graphics 640", "Intel(R) Iris(TM) Pro Graphics 6200", "NVIDIA GeForce 840M", "NVIDIA GeForce 920MX", "NVIDIA GeForce 930MX",
		"NVIDIA GeForce 940M", "NVIDIA GeForce 940MX", "NVIDIA GeForce GT 1030", "NVIDIA GeForce GT 545", "NVIDIA GeForce GT 555M", "NVIDIA GeForce GT 635M", "NVIDIA GeForce GT 640", "NVIDIA GeForce GT 640M", "NVIDIA GeForce GT 650M OpenGL Engine", "NVIDIA GeForce GT 650M",
		"NVIDIA GeForce GT 730", "NVIDIA GeForce GT 740", "NVIDIA GeForce GT 740M", "NVIDIA GeForce GT 750M OpenGL Engine", "NVIDIA GeForce GT 750M", "NVIDIA GeForce GT 755M OpenGL Engine", "NVIDIA GeForce GT 755M", "NVIDIA GeForce GTS 450", "NVIDIA GeForce GTX 1050 Ti", "NVIDIA GeForce GTX 1050",
		"NVIDIA GeForce GTX 1060 3GB", "NVIDIA GeForce GTX 1060 5GB", "NVIDIA GeForce GTX 1060 6GB", "NVIDIA GeForce GTX 1060 with Max-Q Design", "NVIDIA GeForce GTX 1060", "NVIDIA GeForce GTX 1070 Ti", "NVIDIA GeForce GTX 1070", "NVIDIA GeForce GTX 1080 Ti", "NVIDIA GeForce GTX 1080", "NVIDIA GeForce GTX 260",
		"NVIDIA GeForce GTX 275", "NVIDIA GeForce GTX 285", "NVIDIA GeForce GTX 460 SE", "NVIDIA GeForce GTX 460", "NVIDIA GeForce GTX 460M", "NVIDIA GeForce GTX 465", "NVIDIA GeForce GTX 470", "NVIDIA GeForce GTX 480", "NVIDIA GeForce GTX 550 Ti", "NVIDIA GeForce GTX 555",
		"NVIDIA GeForce GTX 560 SE", "NVIDIA GeForce GTX 560 Ti", "NVIDIA GeForce GTX 560", "NVIDIA GeForce GTX 560M", "NVIDIA GeForce GTX 570", "NVIDIA GeForce GTX 570M", "NVIDIA GeForce GTX 580", "NVIDIA GeForce GTX 590", "NVIDIA GeForce GTX 645", "NVIDIA GeForce GTX 650 Ti BOOST",
		"NVIDIA GeForce GTX 650 Ti", "NVIDIA GeForce GTX 650", "NVIDIA GeForce GTX 660 Ti", "NVIDIA GeForce GTX 660", "NVIDIA GeForce GTX 660M OpenGL Engine", "NVIDIA GeForce GTX 660M", "NVIDIA GeForce GTX 670", "NVIDIA GeForce GTX 670M", "NVIDIA GeForce GTX 670MX", "NVIDIA GeForce GTX 675M",
		"NVIDIA GeForce GTX 675MX OpenGL Engine", "NVIDIA GeForce GTX 675MX", "NVIDIA GeForce GTX 680", "NVIDIA GeForce GTX 680M", "NVIDIA GeForce GTX 680MX OpenGL Engine", "NVIDIA GeForce GTX 690", "NVIDIA GeForce GTX 745", "NVIDIA GeForce GTX 750 Ti", "NVIDIA GeForce GTX 750", "NVIDIA GeForce GTX 760 (192-bit)",
		"NVIDIA GeForce GTX 760 Ti OEM", "NVIDIA GeForce GTX 760", "NVIDIA GeForce GTX 760M", "NVIDIA GeForce GTX 765M", "NVIDIA GeForce GTX 770", "NVIDIA GeForce GTX 770M", "NVIDIA GeForce GTX 775M OpenGL Engine", "NVIDIA GeForce GTX 780 Ti", "NVIDIA GeForce GTX 780", "NVIDIA GeForce GTX 780M OpenGL Engine",
		"NVIDIA GeForce GTX 780M", "NVIDIA GeForce GTX 850M", "NVIDIA GeForce GTX 860M", "NVIDIA GeForce GTX 870M", "NVIDIA GeForce GTX 880M", "NVIDIA GeForce GTX 950", "NVIDIA GeForce GTX 950M", "NVIDIA GeForce GTX 960", "NVIDIA GeForce GTX 960M", "NVIDIA GeForce GTX 965M",
		"NVIDIA GeForce GTX 970", "NVIDIA GeForce GTX 970M", "NVIDIA GeForce GTX 980 Ti", "NVIDIA GeForce GTX 980", "NVIDIA GeForce GTX 980M", "NVIDIA GeForce GTX TITAN Black", "NVIDIA GeForce GTX TITAN X", "NVIDIA GeForce GTX TITAN", "NVIDIA GeForce MX150", "NVIDIA GeForce RTX 2070",
		"NVIDIA GeForce RTX 2080", "NVIDIA Quadro K2100M", "NVIDIA Tesla P40", "Radeon (TM) RX 470 Graphics", "Radeon (TM) RX 480 Graphics", "Radeon RX 550 Series", "Radeon RX 560 Series", "Radeon RX 570 Series", "Radeon RX 580 Series", "Radeon(TM) RX 460 Graphics"
	};

	private readonly HashSet<string> blackList = new HashSet<string>
	{
		"AMD Mobility Radeon HD 5000 Series", "AMD Radeon HD 5450", "AMD Radeon HD 5570", "AMD Radeon HD 6310 Graphics", "AMD Radeon HD 6320 Graphics", "AMD RADEON HD 6450", "AMD Radeon HD 6450", "AMD Radeon HD 6520G", "AMD Radeon HD 6530D Graphics", "AMD Radeon HD 6530D",
		"AMD Radeon HD 6570", "AMD Radeon HD 6670", "AMD Radeon HD 7310 Graphics", "AMD Radeon HD 7340 Graphics", "AMD Radeon HD 7420G", "AMD Radeon HD 7450", "AMD Radeon HD 7480D", "AMD Radeon HD 7500 Series", "AMD Radeon HD 7500M/7600M Series", "AMD Radeon HD 7520G",
		"AMD Radeon HD 7540D", "AMD Radeon HD 7560D", "AMD Radeon HD 7570", "AMD Radeon HD 7600G", "AMD Radeon HD 7640G", "AMD Radeon HD 8210", "AMD Radeon HD 8240", "AMD Radeon HD 8330", "AMD Radeon HD 8370D", "AMD Radeon HD 8400",
		"AMD Radeon HD 8470D", "AMD Radeon HD 8510G", "AMD Radeon HD 8550G", "AMD Radeon HD 8610G", "AMD Radeon(TM) HD 6480G", "AMD Radeon(TM) HD 6520G", "AMD Radeon(TM) HD 6620G", "AMD Radeon(TM) HD 8510G", "AMD Radeon(TM) HD 8610G", "AMD Radeon(TM) R2 Graphics",
		"AMD Radeon(TM) R3 Graphics", "AMD Radeon(TM) R4 Graphics", "AMD Radeon(TM) R5 Graphics", "AMD Radeon(TM) R7 Graphics", "ATI Mobility Radeon HD 4200 Series", "ATI Mobility Radeon HD 5470", "ATI Mobility Radeon HD 5650", "ATI Radeon HD 4200", "ATI Radeon HD 4300/4500 Series", "ATI Radeon HD 4600 Series",
		"ATI Radeon HD 4800 Series", "ATI Radeon HD 5450", "ATI Radeon HD 5570", "Intel(R) G33/G31 Express Chipset Family", "Intel(R) HD Graphics 3000", "Intel(R) HD Graphics 4000", "Intel(R) HD Graphics 4400", "Intel(R) HD Graphics 5000", "Intel(R) HD Graphics Family", "Intel(R) HD Graphics",
		"Intel(R) Iris(TM) Graphics 5100", "Microsoft Basic Render Driver", "Mobile Intel(R) 4 Series Express Chipset Family", "Mobile Intel(R) HD Graphics", "NVIDIA GeForce 210", "NVIDIA GeForce 310M", "NVIDIA GeForce 610M", "NVIDIA GeForce 820M", "NVIDIA GeForce 8400GS", "NVIDIA GeForce 8600 GT",
		"NVIDIA GeForce 8800 GT", "NVIDIA GeForce 9500 GT", "NVIDIA GeForce 9600 GT", "NVIDIA GeForce 9800 GT", "NVIDIA GeForce 9800 GTX/9800 GTX+", "NVIDIA GeForce GPU", "NVIDIA GeForce GT 220", "NVIDIA GeForce GT 240", "NVIDIA GeForce GT 240M", "NVIDIA GeForce GT 320",
		"NVIDIA GeForce GT 330M", "NVIDIA GeForce GT 430", "NVIDIA GeForce GT 440", "NVIDIA GeForce GT 520", "NVIDIA GeForce GT 520M", "NVIDIA GeForce GT 525M", "NVIDIA GeForce GT 530", "NVIDIA GeForce GT 540M", "NVIDIA GeForce GT 610", "NVIDIA GeForce GT 620",
		"NVIDIA GeForce GT 630", "NVIDIA GeForce GT 630M", "NVIDIA GeForce GT 635", "NVIDIA GeForce GT 720", "NVIDIA GeForce GT 720M", "NVIDIA GeForce GTS 250", "NVIDIA GeForce GTX 260M", "Parallels Display Adapter (WDDM)"
	};

	private void Start()
	{
		string text = SystemInfo.graphicsDeviceName.Trim();
		int systemMemorySize = SystemInfo.systemMemorySize;
		int processorCount = SystemInfo.processorCount;
		string text2 = SystemInfo.processorType.Trim();
		GPUClass gPUClass = (whiteList.Contains(text) ? GPUClass.White : (blackList.Contains(text) ? GPUClass.Black : GPUClass.Unknown));
		bool flag = systemMemorySize < 3584;
		bool flag2 = processorCount < 2;
		bool flag3 = gPUClass == GPUClass.Black;
		if (flag || flag2 || flag3)
		{
			using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
			{
				StringBuilder sb = stringBuilderPool.sb;
				sb.Append("MinSpec Warning: Machine appears to be below minimum specification. Presenting warning in main menu.");
				sb.AppendFormat("\nCPU logical processors: {0}", processorCount);
				sb.AppendFormat("\nCPU name: {0}", text2);
				sb.AppendFormat("\nGPU name: {0}", text);
				sb.AppendFormat("\nGPU class: {0}", gPUClass);
				sb.AppendFormat("\nRAM: {0}", systemMemorySize);
				Debug.Log(sb.ToString());
			}
			mainMenu.SetActive(value: false);
			OpenPanel("Home");
			cpuDetected.text = text2;
			cpuL.text = processorCount.ToString();
			gpuDetected.text = text;
			ramDetected.text = systemMemorySize.ToString("#,##0") + " megabytes";
			ramOK.SetActive(!flag);
			ramNOK.SetActive(flag);
			cpuOK.SetActive(!flag2);
			cpuNOK.SetActive(flag2);
			gpuOK.SetActive(gPUClass == GPUClass.White);
			gpuNOK.SetActive(gPUClass == GPUClass.Black);
			gpuUNK.SetActive(gPUClass == GPUClass.Unknown);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void OpenPanel(string target)
	{
		GameObject[] array = panels;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(gameObject.name == target);
			}
		}
	}

	public void Dismiss()
	{
		mainMenu.SetActive(value: true);
		Object.Destroy(base.gameObject);
		uGUI_MainMenu.main.Select();
	}

	public void OpenSupportForums()
	{
		PlatformUtils.OpenURL("http://steamcommunity.com/app/264710/discussions/", overlay: true);
	}
}
