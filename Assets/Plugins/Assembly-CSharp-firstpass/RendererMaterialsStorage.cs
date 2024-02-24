using UnityEngine;

public class RendererMaterialsStorage
{
	private Material[] initialMaterials;

	private Material[] currentMaterials;

	public Renderer renderer { get; private set; }

	public int Count => initialMaterials.Length;

	public RendererMaterialsStorage(Renderer renderer)
	{
		this.renderer = renderer;
		initialMaterials = renderer.sharedMaterials;
		currentMaterials = (Material[])initialMaterials.Clone();
	}

	public Material GetInitialMaterial(int index)
	{
		return initialMaterials[index];
	}

	public void GetCurrentMaterial(int index, out Material material, out bool isCopied)
	{
		material = currentMaterials[index];
		Material material2 = initialMaterials[index];
		isCopied = material != material2;
	}

	public bool TryGetCopiedMaterial(int index, out Material material)
	{
		GetCurrentMaterial(index, out var material2, out var isCopied);
		if (isCopied)
		{
			material = material2;
			return true;
		}
		material = null;
		return false;
	}

	public Material GetOrCreateCopiedMaterial(int index, bool applyNowIfCreated = true)
	{
		GetCurrentMaterial(index, out var material, out var isCopied);
		Material material2;
		if (isCopied)
		{
			material2 = material;
		}
		else
		{
			material2 = new Material(initialMaterials[index]);
			currentMaterials[index] = material2;
			if (applyNowIfCreated)
			{
				ApplyCurrentMaterials();
			}
		}
		return material2;
	}

	public void ApplyCurrentMaterials()
	{
		if (renderer != null)
		{
			renderer.materials = currentMaterials;
		}
	}

	public void EnsureAllCopiedMaterials()
	{
		for (int i = 0; i < Count; i++)
		{
			GetOrCreateCopiedMaterial(i);
		}
	}

	public void DestroyCopiedAndRestoreInitialMaterials()
	{
		for (int i = 0; i < Count; i++)
		{
			if (TryGetCopiedMaterial(i, out var material))
			{
				if (material != null)
				{
					Object.Destroy(material);
				}
				currentMaterials[i] = initialMaterials[i];
			}
		}
		ApplyCurrentMaterials();
	}
}
