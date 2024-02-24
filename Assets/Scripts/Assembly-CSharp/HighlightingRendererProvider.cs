using System;
using System.Collections.Generic;
using UnityEngine;

public class HighlightingRendererProvider : MonoBehaviour
{
	[Serializable]
	public class Submesh
	{
		public Renderer renderer;

		public List<int> indices;
	}

	public List<Submesh> submeshes;
}
