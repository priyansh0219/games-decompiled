using UnityEngine;

[ExecuteInEditMode]
public class WaterPlane : MonoBehaviour
{
	public Vector2 size;

	private float meshSpacing = 1f;

	public Material material;

	private void Start()
	{
		CreateGeometry();
	}

	private static Mesh CreatePlaneMesh(float width, float height, float meshSpacing)
	{
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.HideAndDontSave;
		mesh.name = "WaterPlane";
		int num = Mathf.CeilToInt(width / meshSpacing);
		int num2 = Mathf.CeilToInt(height / meshSpacing);
		Vector3[] array = new Vector3[(num + 1) * (num2 + 1)];
		int num3 = 0;
		for (int i = 0; i <= num2; i++)
		{
			int num4 = 0;
			while (num4 <= num)
			{
				array[num3] = new Vector3((float)num4 * width / (float)num, 0f, (float)i * height / (float)num2);
				num4++;
				num3++;
			}
		}
		mesh.vertices = array;
		int[] array2 = new int[num * num2 * 6];
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		while (num7 < num2)
		{
			int num8 = 0;
			while (num8 < num)
			{
				array2[num5] = num6;
				array2[num5 + 3] = (array2[num5 + 2] = num6 + 1);
				array2[num5 + 4] = (array2[num5 + 1] = num6 + num + 1);
				array2[num5 + 5] = num6 + num + 2;
				num8++;
				num5 += 6;
				num6++;
			}
			num7++;
			num6++;
		}
		mesh.triangles = array2;
		mesh.RecalculateNormals();
		return mesh;
	}

	private void CreateGeometry()
	{
		MeshFilter meshFilter = base.gameObject.EnsureComponent<MeshFilter>();
		meshFilter.hideFlags = HideFlags.HideAndDontSave;
		meshFilter.mesh = CreatePlaneMesh(size.x, size.y, meshSpacing);
		MeshRenderer meshRenderer = base.gameObject.EnsureComponent<MeshRenderer>();
		meshRenderer.hideFlags = HideFlags.HideAndDontSave;
		meshRenderer.material = material;
	}
}
