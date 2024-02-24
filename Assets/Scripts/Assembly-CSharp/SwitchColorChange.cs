using System.Collections.Generic;
using UnityEngine;

public class SwitchColorChange : MonoBehaviour
{
	public Color startColor = Color.red;

	public Color endColor = Color.green;

	public GameObject[] switches = new GameObject[3];

	private List<Material> mats = new List<Material>();

	private int switchInt;

	private void Start()
	{
		Initialize();
	}

	private void Initialize()
	{
		for (int i = 0; i < switches.Length; i++)
		{
			if (switches[i] != null)
			{
				Material material = switches[i].GetComponent<SkinnedMeshRenderer>().material;
				material.SetColor(ShaderPropertyID._GlowColor, startColor);
				mats.Add(material);
			}
		}
		switchInt = 0;
	}

	public void SwapSwitchColor()
	{
		if (mats[switchInt] != null)
		{
			mats[switchInt].SetColor(ShaderPropertyID._GlowColor, endColor);
		}
		switchInt++;
	}
}
