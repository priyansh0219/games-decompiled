using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class BiomePlot : MonoBehaviour
	{
		public bool export;

		public float minDepth;

		public float maxDepth;

		public float minTemp;

		public float maxTemp;

		public float startDistance;

		public float size;

		public Color drawColor;

		public TextMesh textMesh;

		public string tileSet;

		public List<BiomeEntityDistribution> entityDistributions = new List<BiomeEntityDistribution>();

		public void UpdateBiomePlotInternals()
		{
			if ((bool)textMesh)
			{
				if (export)
				{
					textMesh.text = base.gameObject.name;
				}
				else
				{
					textMesh.text = "(" + base.gameObject.name + ")";
				}
				textMesh.gameObject.transform.localScale = new Vector3(0.005f / base.gameObject.transform.localScale.x, 0.005f / base.gameObject.transform.localScale.y, 1f);
				Debug.Log("Setting textMesh scale to: " + textMesh.gameObject.transform.localScale);
			}
			minTemp = base.transform.position.x - base.transform.localScale.x;
			maxTemp = base.transform.position.x + base.transform.localScale.x;
			minDepth = 1f - (base.transform.position.y + base.transform.localScale.y);
			maxDepth = 1f - (base.transform.position.y - base.transform.localScale.y);
			float a = ((!export) ? 0.3f : 1f);
			base.gameObject.GetComponent<Renderer>().material.color = new Color(drawColor.r, drawColor.g, drawColor.b, a);
		}

		public void UpdateGameObject()
		{
			float x = (minTemp + maxTemp) / 2f;
			float num = (maxTemp - minTemp) / 2f;
			float y = 1f - (minDepth + maxDepth) / 2f;
			float num2 = (maxDepth - minDepth) / 2f;
			float z = num * num2 * 5f;
			base.gameObject.transform.position = new Vector3(x, y, z);
			base.gameObject.transform.localScale = new Vector3(num, num2, 1f);
			if ((bool)textMesh)
			{
				Transform transform = textMesh.gameObject.transform;
				transform.position = new Vector3(transform.position.x, transform.position.y, -0.5f);
			}
		}

		public bool ValidateEntityDistributions(out string errorString)
		{
			errorString = "No entities found";
			if (entityDistributions.Count > 0)
			{
				for (int i = 0; i < entityDistributions.Count; i++)
				{
					if (!entityDistributions[i].IsValid(out errorString))
					{
						errorString = errorString + " (" + base.gameObject.name + ")";
						return false;
					}
				}
				return true;
			}
			errorString = "Biome " + base.gameObject.name + " has 0 entity distributions.";
			return false;
		}
	}
}
