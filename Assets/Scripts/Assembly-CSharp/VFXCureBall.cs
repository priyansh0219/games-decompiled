using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VFXCureBall : MonoBehaviour
{
	public Transform target;

	[AssertNotNull]
	public Renderer rend;

	[AssertNotNull]
	public Transform motionTransform;

	[AssertNotNull]
	public AnimationCurve ripplesCurve;

	public float sequenceDuration = 21.875f;

	public bool reactToRightHand = true;

	public int delay = 5;

	public float smoothing = 0.15f;

	private float animTime;

	private bool sequenceIsPlaying;

	private MaterialPropertyBlock block;

	private Vector3 prevSmoothedPos = Vector3.zero;

	private List<Vector3> posList;

	private void OnEnable()
	{
		Init();
	}

	private void Init()
	{
		block = new MaterialPropertyBlock();
		posList = new List<Vector3>();
		if (Player.main != null)
		{
			target = (reactToRightHand ? Player.main.armsController.rightHand : Player.main.armsController.leftHand);
		}
	}

	public void StartPlayerCureSequence()
	{
		sequenceIsPlaying = true;
	}

	private void Update()
	{
		if (rend == null)
		{
			return;
		}
		if (block == null)
		{
			Init();
		}
		if (sequenceIsPlaying)
		{
			animTime += Time.deltaTime / sequenceDuration;
			if (animTime > 1f)
			{
				animTime = 0f;
				sequenceIsPlaying = false;
			}
		}
		if (posList.Count < delay)
		{
			for (int i = 0; i < delay; i++)
			{
				posList.Add(target.position);
			}
		}
		posList.Add(target.position);
		Vector3 vector = (prevSmoothedPos = Vector3.Lerp(prevSmoothedPos, posList[0], smoothing));
		posList.RemoveAt(0);
		block.Clear();
		rend.GetPropertyBlock(block);
		block.SetVector("_RipplePos1", new Vector4(vector.x, vector.y, vector.z, ripplesCurve.Evaluate(animTime)));
		rend.SetPropertyBlock(block);
	}
}
