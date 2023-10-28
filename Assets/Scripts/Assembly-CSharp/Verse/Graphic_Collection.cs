using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class Graphic_Collection : Graphic
	{
		protected Graphic[] subGraphics;

		public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
		{
			Graphic[] array = subGraphics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].TryInsertIntoAtlas(groupKey);
			}
		}

		public override void Init(GraphicRequest req)
		{
			data = req.graphicData;
			if (req.path.NullOrEmpty())
			{
				throw new ArgumentNullException("folderPath");
			}
			if (req.shader == null)
			{
				throw new ArgumentNullException("shader");
			}
			path = req.path;
			maskPath = req.maskPath;
			color = req.color;
			colorTwo = req.colorTwo;
			drawSize = req.drawSize;
			List<(Texture2D, string)> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
				where !x.name.EndsWith(Graphic_Single.MaskSuffix)
				orderby x.name
				select (x, x.name.Split('_')[0])).ToList();
			if (list.NullOrEmpty())
			{
				Log.Error("Collection cannot init: No textures found at path " + req.path);
				subGraphics = new Graphic[1] { BaseContent.BadGraphic };
				return;
			}
			List<Graphic> list2 = new List<Graphic>();
			foreach (IGrouping<string, (Texture2D, string)> item in from s in list
				group s by s.Item2)
			{
				List<(Texture2D, string)> list3 = item.ToList();
				string text = req.path + "/" + item.Key;
				bool flag = false;
				for (int num = list3.Count - 1; num >= 0; num--)
				{
					if (list3[num].Item1.name.Contains("_east") || list3[num].Item1.name.Contains("_north") || list3[num].Item1.name.Contains("_west") || list3[num].Item1.name.Contains("_south"))
					{
						list3.RemoveAt(num);
						flag = true;
					}
				}
				if (list3.Count > 0)
				{
					foreach (var item2 in list3)
					{
						list2.Add(GraphicDatabase.Get(typeof(Graphic_Single), req.path + "/" + item2.Item1.name, req.shader, drawSize, color, colorTwo, data, req.shaderParameters));
					}
				}
				if (flag)
				{
					list2.Add(GraphicDatabase.Get(typeof(Graphic_Multi), text, req.shader, drawSize, color, colorTwo, data, req.shaderParameters));
				}
			}
			subGraphics = list2.ToArray();
		}
	}
}
