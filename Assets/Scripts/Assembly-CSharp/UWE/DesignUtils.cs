using System;
using System.IO;
using UnityEngine;

namespace UWE
{
	public static class DesignUtils
	{
		public static void ChartBiomeTempVsDepth()
		{
			object[] array = UnityEngine.Object.FindSceneObjectsOfType(typeof(GameObject));
			array = array;
			for (int i = 0; i < array.Length; i++)
			{
				BiomePlot component = ((GameObject)array[i]).GetComponent<BiomePlot>();
				if ((bool)component)
				{
					component.UpdateGameObject();
				}
			}
		}

		public static bool ExportBiomeCSV(string exportPath, out string statusString)
		{
			bool flag = false;
			int num = 0;
			statusString = "";
			using (StreamWriter streamWriter = FileUtils.CreateTextFile(exportPath))
			{
				streamWriter.WriteLine("name,superBiome,idealDepth,idealHeightfield,idealStartDistance,tileSetScene,fillerClass");
				object[] array = UnityEngine.Object.FindSceneObjectsOfType(typeof(GameObject));
				array = array;
				for (int i = 0; i < array.Length; i++)
				{
					BiomePlot component = ((GameObject)array[i]).GetComponent<BiomePlot>();
					if (!component || !component.export)
					{
						continue;
					}
					component.UpdateBiomePlotInternals();
					string text = Mathf.FloorToInt((component.minDepth + component.maxDepth) / 2f * 1000f).ToString();
					string text2 = component.tileSet.Replace(Environment.NewLine, "");
					if (text2 != "")
					{
						if (File.Exists(text2))
						{
							if (component.ValidateEntityDistributions(out statusString))
							{
								streamWriter.WriteLine(component.gameObject.name + ",none," + text + ",0,0," + text2 + ",SparseCaves");
								num++;
							}
							else
							{
								Debug.Log("Got error: " + statusString);
								flag = true;
							}
						}
						else
						{
							statusString = "Biome tileset \"" + text2 + "\" doesn't exist on disk.";
							flag = true;
						}
					}
					else
					{
						statusString = "Biome plot \"" + component.name + "\" has missing tileset.";
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				streamWriter.Close();
			}
			if (!flag)
			{
				if (num > 0)
				{
					statusString = "Saved " + num + " biomes to " + exportPath + ".";
				}
				else
				{
					statusString = "Couldn't save to " + exportPath + ".";
				}
			}
			return !flag;
		}

		public static bool ExportEntityCSV(string exportPath, out string statusString)
		{
			bool flag = false;
			int num = 0;
			statusString = "";
			using (StreamWriter streamWriter = FileUtils.CreateTextFile(exportPath))
			{
				streamWriter.WriteLine("biome,prefabName,count,concentration,difficulty");
				object[] array = UnityEngine.Object.FindSceneObjectsOfType(typeof(GameObject));
				array = array;
				for (int i = 0; i < array.Length; i++)
				{
					BiomePlot component = ((GameObject)array[i]).GetComponent<BiomePlot>();
					if ((bool)component && component.export)
					{
						for (int j = 0; j < component.entityDistributions.Count; j++)
						{
							BiomeEntityDistribution biomeEntityDistribution = component.entityDistributions[j];
							streamWriter.WriteLine(component.gameObject.name + "," + biomeEntityDistribution.entityName + "," + biomeEntityDistribution.count + ",0,0");
							num++;
						}
					}
				}
				streamWriter.Close();
			}
			if (num > 0)
			{
				statusString = "Saved " + num + " entityDescriptions to " + exportPath + ".";
			}
			else
			{
				statusString = "Couldn't save to " + exportPath + ".";
				flag = true;
			}
			return !flag;
		}
	}
}
