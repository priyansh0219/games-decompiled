using System.IO;
using UWE;
using UnityEngine;

namespace WorldBuilding
{
	public struct BiomeMapData
	{
		private const int DimensionsFactor = 64;

		private byte[] indexData;

		private BiomeProperties[] biomeProperties;

		private int width;

		private int height;

		private int worldToBiomeMapFactor;

		public void Destroy()
		{
			indexData = null;
			biomeProperties = null;
		}

		public bool IsLoaded()
		{
			if (indexData != null && biomeProperties != null && indexData.Length != 0)
			{
				return biomeProperties.Length != 0;
			}
			return false;
		}

		private int CalculateWorldBlockToIndex(Int2 block)
		{
			return block.y / worldToBiomeMapFactor * width + block.x / worldToBiomeMapFactor;
		}

		private bool IsIndexInRange(int i)
		{
			if (IsLoaded() && i >= 0)
			{
				return i < indexData.Length;
			}
			return false;
		}

		public bool TryGetBiomeProperties(Int2 block, out BiomeProperties biomeProperties)
		{
			using (new ProfilingUtils.Sample("BiomeMapData.TryGetBiomeProperties"))
			{
				if (TryGetBiomeIndexForCoords(block, out var index))
				{
					biomeProperties = this.biomeProperties[index];
					return true;
				}
				biomeProperties = new BiomeProperties();
				return false;
			}
		}

		public bool TryGetBiomeIndexForCoords(Int2 block, out int index)
		{
			if (TryCalculateWorldBlockToIndex(block, out var i))
			{
				index = GetBiomeIndexForCoords(i);
				return true;
			}
			index = -1;
			return false;
		}

		private bool TryCalculateWorldBlockToIndex(Int2 block, out int i)
		{
			if (IsLoaded())
			{
				int num = CalculateWorldBlockToIndex(block);
				if (IsIndexInRange(num))
				{
					i = num;
					return true;
				}
			}
			i = 0;
			return false;
		}

		private int GetBiomeIndexForCoords(int i)
		{
			return indexData[i];
		}

		public void Load(string biomeMapPath, string biomesCSVPath, int landSizeX)
		{
			using (new ProfilingUtils.Sample("BiomeMapData.Load"))
			{
				indexData = LoadIndexData(biomeMapPath, out width, out height);
				worldToBiomeMapFactor = landSizeX / width;
				Debug.LogFormat("biome map downsample factor: {0}", worldToBiomeMapFactor);
				biomeProperties = LoadBiomeProperties(biomesCSVPath);
			}
		}

		public static byte[] LoadIndexData(string biomeMapPath, out int mapWidth, out int mapHeight)
		{
			using (new ProfilingUtils.Sample("BiomeMapData.LoadIndexData"))
			{
				byte[] array = File.ReadAllBytes(biomeMapPath);
				mapWidth = array[array.Length - 2] * 64;
				mapHeight = array[array.Length - 1] * 64;
				return array;
			}
		}

		public static BiomeProperties[] LoadBiomeProperties(string biomesCSVPath)
		{
			using (new ProfilingUtils.Sample("BiomeMapData.LoadBiomeProperties"))
			{
				return CSVUtils.Load<BiomeProperties>(biomesCSVPath).ToArray();
			}
		}
	}
}
