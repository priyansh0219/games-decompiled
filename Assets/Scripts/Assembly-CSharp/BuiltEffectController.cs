using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BuiltEffectController : MonoBehaviour
{
	private const string materialPath = "Materials/builtfade.mat";

	private const string propertyName = "_FadeAmount";

	private static Material originalMaterial;

	public float duration = 1f;

	private Renderer[] renderers;

	private Sequence sequence;

	private Material material;

	private int propertyID;

	private void Awake()
	{
		sequence = new Sequence();
		propertyID = Shader.PropertyToID("_FadeAmount");
	}

	private IEnumerator Start()
	{
		if (originalMaterial == null)
		{
			AsyncOperationHandle<Material> loadRequest = AddressablesUtility.LoadAsync<Material>("Materials/builtfade.mat");
			yield return loadRequest;
			loadRequest.LogExceptionIfFailed("Materials/builtfade.mat");
			originalMaterial = loadRequest.Result;
		}
		material = new Material(originalMaterial);
		renderers = MaterialExtensions.AssignMaterial(base.gameObject, material);
		sequence.Set(duration, current: false, target: true, DestroyCallback);
	}

	private void Update()
	{
		sequence.Update();
		if (material != null && renderers != null)
		{
			MaterialExtensions.SetFloat(renderers, propertyID, sequence.t);
		}
	}

	private void DestroyCallback()
	{
		Object.Destroy(base.gameObject);
	}
}
