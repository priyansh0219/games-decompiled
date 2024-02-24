using System;
using System.Collections.Generic;
using Gendarme;

namespace UnityEngine.PostProcessing
{
	[SuppressMessage("Gendarme.Rules.Correctness", "DisposableFieldsShouldBeDisposedRule")]
	[SuppressMessage("Gendarme.Rules.Performance", "PreferCharOverloadRule")]
	[SuppressMessage("Gendarme.Rules.Globalization", "PreferStringComparisonOverrideRule")]
	public sealed class MaterialFactory : IDisposable
	{
		private Dictionary<string, Material> m_Materials;

		public MaterialFactory()
		{
			m_Materials = new Dictionary<string, Material>();
		}

		public Material Get(string shaderName)
		{
			if (!m_Materials.TryGetValue(shaderName, out var value))
			{
				Shader shader = Shader.Find(shaderName);
				if (shader == null)
				{
					throw new ArgumentException($"Shader not found ({shaderName})");
				}
				value = new Material(shader)
				{
					name = string.Format("PostFX - {0}", shaderName.Substring(shaderName.LastIndexOf("/") + 1)),
					hideFlags = HideFlags.DontSave
				};
				m_Materials.Add(shaderName, value);
			}
			return value;
		}

		public void Dispose()
		{
			Dictionary<string, Material>.Enumerator enumerator = m_Materials.GetEnumerator();
			while (enumerator.MoveNext())
			{
				GraphicsUtils.Destroy(enumerator.Current.Value);
			}
			m_Materials.Clear();
		}
	}
}
