using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class Pipe : MonoBehaviour
{
	private static List<Pipe> pipes = new List<Pipe>();

	private static List<Pipe> rootPipes = new List<Pipe>();

	private static bool convertPipes = true;

	[AssertNotNull]
	public GameObject newPipePrefab;

	[AssertNotNull]
	public GameObject floaterPrefab;

	[ProtoMember(1)]
	public string parentPipeUID;

	private List<Pipe> children = new List<Pipe>();

	private string rootpipeUID;

	private bool isRoot;

	private void Start()
	{
		pipes.Add(this);
		if (string.IsNullOrEmpty(parentPipeUID))
		{
			isRoot = base.transform.position.y > -2f;
			rootPipes.Add(this);
		}
		if (convertPipes)
		{
			Invoke("ConvertPipes", 2f);
			convertPipes = false;
		}
		base.name = "PipeOld";
	}

	private void CreateNewPipe(IPipeConnection parent)
	{
		if (isRoot)
		{
			PipeSurfaceFloater component = Object.Instantiate(floaterPrefab).GetComponent<PipeSurfaceFloater>();
			component.transform.position = base.transform.position;
			component.deployed = true;
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(component.rigidBody, isKinematic: true);
			parent = component;
		}
		else
		{
			IPipeConnection component2 = Object.Instantiate(newPipePrefab).GetComponent<IPipeConnection>();
			component2.GetGameObject().transform.position = base.transform.position;
			component2.SetParent(parent);
			parent = component2;
		}
		for (int i = 0; i < children.Count; i++)
		{
			children[i].CreateNewPipe(parent);
		}
	}

	private static Pipe GetPipeById(string id)
	{
		for (int i = 0; i < pipes.Count; i++)
		{
			if (string.Compare(id, pipes[i].GetComponent<UniqueIdentifier>().Id) == 0)
			{
				return pipes[i];
			}
		}
		return null;
	}

	private void ConvertPipes()
	{
		for (int i = 0; i < pipes.Count; i++)
		{
			Pipe pipeById = GetPipeById(pipes[i].parentPipeUID);
			if (pipeById != null)
			{
				pipeById.children.Add(pipes[i]);
			}
		}
		for (int j = 0; j < rootPipes.Count; j++)
		{
			rootPipes[j].CreateNewPipe(null);
		}
		for (int k = 0; k < pipes.Count; k++)
		{
			Object.Destroy(pipes[k].gameObject);
		}
	}
}
