using System.Collections.Generic;
using UnityEngine;

public class DestroyDuplicates : MonoBehaviour, ICompileTimeCheckable
{
	private static readonly Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

	[AssertNotNull]
	public PrefabIdentifier identifier;

	private string classId = string.Empty;

	private bool registered;

	private void Start()
	{
		classId = identifier.ClassId;
		if (objects.TryGetValue(classId, out var _))
		{
			Debug.LogWarningFormat(this, "Duplicate prefab instance {0} ('{1}') detected. Destroying.", base.name, classId);
			Object.Destroy(base.gameObject);
		}
		else
		{
			objects.Add(classId, base.gameObject);
			registered = true;
		}
	}

	private void OnDestroy()
	{
		if (registered)
		{
			objects.Remove(classId);
		}
	}

	public string CompileTimeCheck()
	{
		if (base.gameObject != identifier.gameObject)
		{
			return "Link to PrefabIdentifier must be on same object";
		}
		return null;
	}
}
