using UnityEngine;
using UnityEngine.UI;

public class uGUI_Icon : MaskableGraphic
{
	private static readonly Vector4 sDefaultTangent = new Vector4(1f, 0f, 0f, -1f);

	private static readonly Vector3 sDefaultNormal = new Vector3(0f, 0f, -1f);

	protected static readonly Vector2[] sVertScratch = new Vector2[4];

	protected static readonly Vector2[] sUV0Scratch = new Vector2[4];

	protected Sprite _sprite;

	public Sprite sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			if (_sprite != value)
			{
				_sprite = value;
				SetAllDirty();
			}
		}
	}

	public override Texture mainTexture
	{
		get
		{
			if (_sprite != null)
			{
				Texture2D texture = _sprite.texture;
				if (texture != null)
				{
					return texture;
				}
			}
			return Graphic.s_WhiteTexture;
		}
	}

	public float pixelsPerUnit
	{
		get
		{
			float num = ((sprite != null) ? sprite.pixelsPerUnit : 100f);
			float num2 = ((base.canvas != null) ? base.canvas.referencePixelsPerUnit : 100f);
			return num / num2;
		}
	}

	protected uGUI_Icon()
	{
		base.useLegacyMeshGeneration = false;
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if (sprite == null)
		{
			GenerateQuadMesh(vh);
		}
		else
		{
			GeneratePackedMesh(vh);
		}
	}

	protected void GenerateQuadMesh(VertexHelper vh)
	{
		_ = pixelsPerUnit;
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		Vector2 vector = new Vector2(pixelAdjustedRect.x, pixelAdjustedRect.y);
		Vector2 vector2 = new Vector2(pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height);
		Vector2 uv0Min = new Vector2(0f, 0f);
		Vector2 uv0Max = new Vector2(1f, 1f);
		Vector2 uv1Min = vector;
		Vector2 uv1Max = vector2;
		vh.Clear();
		AddQuad(vh, vector, vector2, color, uv0Min, uv0Max, uv1Min, uv1Max);
	}

	protected void GeneratePackedMesh(VertexHelper vh)
	{
		SpriteManager.SpriteCache spriteCache = SpriteManager.GetSpriteCache(sprite);
		Vector2[] vertices = spriteCache.vertices;
		Vector2[] uv = spriteCache.uv;
		ushort[] triangles = spriteCache.triangles;
		Vector2 size = sprite.rect.size;
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		vh.Clear();
		Vector2 b = new Vector2(pixelAdjustedRect.width / size.x, pixelAdjustedRect.height / size.y) * sprite.pixelsPerUnit;
		Color32 color = this.color;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector2 a = vertices[i];
			a = Vector2.Scale(a, b);
			vh.AddVert(new Vector3(a.x, a.y, 0f), color, uv[i], a, sDefaultNormal, sDefaultTangent);
		}
		for (int j = 0; j < triangles.Length; j += 3)
		{
			vh.AddTriangle(triangles[j], triangles[j + 1], triangles[j + 2]);
		}
	}

	protected static void AddQuad(VertexHelper vh, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uv0Min, Vector2 uv0Max, Vector2 uv1Min, Vector2 uv1Max)
	{
		int currentVertCount = vh.currentVertCount;
		vh.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uv0Min.x, uv0Min.y), new Vector2(uv1Min.x, uv1Min.y), sDefaultNormal, sDefaultTangent);
		vh.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uv0Min.x, uv0Max.y), new Vector2(uv1Min.x, uv1Max.y), sDefaultNormal, sDefaultTangent);
		vh.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uv0Max.x, uv0Max.y), new Vector2(uv1Max.x, uv1Max.y), sDefaultNormal, sDefaultTangent);
		vh.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uv0Max.x, uv0Min.y), new Vector2(uv1Max.x, uv1Min.y), sDefaultNormal, sDefaultTangent);
		vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
		vh.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
	}

	protected Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
	{
		for (int i = 0; i <= 1; i++)
		{
			float num = border[i] + border[i + 2];
			float num2 = rect.size[i];
			if (num2 < num && num != 0f)
			{
				float num3 = num2 / num;
				border[i] *= num3;
				border[i + 2] *= num3;
			}
		}
		return border;
	}

	public override void SetNativeSize()
	{
		if (_sprite != null)
		{
			Vector2 sizeDelta = _sprite.rect.size / pixelsPerUnit;
			base.rectTransform.anchorMax = base.rectTransform.anchorMin;
			base.rectTransform.sizeDelta = sizeDelta;
			SetAllDirty();
		}
	}
}
