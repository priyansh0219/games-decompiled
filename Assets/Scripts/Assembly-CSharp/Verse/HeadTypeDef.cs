using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class HeadTypeDef : Def
	{
		public string graphicPath;

		public Gender gender;

		public bool narrow;

		public Vector2 hairMeshSize = new Vector2(1.5f, 1.5f);

		public Vector2 beardMeshSize = new Vector2(1.5f, 1.5f);

		public Vector3 beardOffset;

		public Vector3? eyeOffsetEastWest;

		public float beardOffsetXEast;

		public float selectionWeight = 1f;

		public bool randomChosen = true;

		public List<GeneDef> requiredGenes;

		[Unsaved(false)]
		private List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

		public Graphic_Multi GetGraphic(Color color, bool dessicated = false, bool skinColorOverriden = false)
		{
			Shader shader = ((!dessicated) ? ShaderUtility.GetSkinShader(skinColorOverriden) : ShaderDatabase.Cutout);
			for (int i = 0; i < graphics.Count; i++)
			{
				if (color.IndistinguishableFrom(graphics[i].Key) && graphics[i].Value.Shader == shader)
				{
					return graphics[i].Value;
				}
			}
			Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPath, shader, Vector2.one, color);
			graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi));
			return graphic_Multi;
		}
	}
}
