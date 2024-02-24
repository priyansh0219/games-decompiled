using UnityEngine;
using UnityEngine.UI;

public class uGUI_TextGradient : MonoBehaviour, IMeshModifier
{
	public Color32 color0 = Color.white;

	public Color32 color1 = Color.white;

	public Color32 color2 = Color.black;

	public Color32 color3 = Color.black;

	public bool affectAlpha;

	public void ModifyMesh(Mesh mesh)
	{
	}

	public void ModifyMesh(VertexHelper vh)
	{
		UIVertex vertex = UIVertex.simpleVert;
		int i = 0;
		for (int currentVertCount = vh.currentVertCount; i < currentVertCount; i++)
		{
			vh.PopulateUIVertex(ref vertex, i);
			Color32 color;
			switch (i % 4)
			{
			case 0:
				color = color0;
				break;
			case 1:
				color = color1;
				break;
			case 2:
				color = color2;
				break;
			default:
				color = color3;
				break;
			}
			if (!affectAlpha)
			{
				color.a = vertex.color.a;
			}
			vertex.color = color;
			vh.SetUIVertex(vertex, i);
		}
	}
}
