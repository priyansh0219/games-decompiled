using UnityEngine;

public class TerrainChanger : MonoBehaviour
{
	public static void AlterTerrain(Vector3 position, int size, float alterHeight)
	{
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain == null)
		{
			return;
		}
		Vector3 terrainRelativePosition = GetTerrainRelativePosition(position);
		int xBase = (int)terrainRelativePosition.x;
		int yBase = (int)terrainRelativePosition.y;
		float[,] heights = activeTerrain.terrainData.GetHeights(xBase, yBase, size, size);
		float num = alterHeight / activeTerrain.terrainData.size.y;
		Debug.Log("alterHeight: " + alterHeight + " => Normalized: " + num);
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				heights[i, j] += num;
			}
		}
		activeTerrain.terrainData.SetHeights(xBase, yBase, heights);
	}

	private static Vector3 GetTerrainRelativePosition(Vector3 position)
	{
		Terrain activeTerrain = Terrain.activeTerrain;
		Vector3 vector = position - activeTerrain.gameObject.transform.position;
		Vector3 vector2 = default(Vector3);
		vector2.x = vector.x / activeTerrain.terrainData.size.x;
		vector2.y = vector.y / activeTerrain.terrainData.size.y;
		vector2.z = vector.z / activeTerrain.terrainData.size.z;
		int heightmapWidth = activeTerrain.terrainData.heightmapWidth;
		int heightmapHeight = activeTerrain.terrainData.heightmapHeight;
		float x = vector2.x * (float)heightmapWidth;
		float y = vector2.z * (float)heightmapHeight;
		return new Vector3(x, y);
	}

	private void OnDestroy()
	{
		Debug.Log("Reverting terrain...");
		Terrain.activeTerrain.terrainData.RefreshPrototypes();
	}
}
