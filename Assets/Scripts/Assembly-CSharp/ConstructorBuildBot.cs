using System.Collections.Generic;
using UWE;
using UnityEngine;

public class ConstructorBuildBot : MonoBehaviour
{
	public static List<ConstructorBuildBot> buildbots = new List<ConstructorBuildBot>();

	public Transform beamOrigin;

	public GameObject constructObject;

	public BuildBotPath path;

	public int atPathIndex;

	public bool flying;

	public int _botId;

	public FMOD_CustomLoopingEmitter buildLoopingSound;

	public FMOD_CustomLoopingEmitter[] hoverLoopSounds;

	private FMOD_CustomLoopingEmitter assignedHoverLoopSound;

	private LineRenderer lineRenderer;

	public Material beamMaterial;

	private bool _launch;

	private bool _usingMenu;

	private bool _building;

	public Vector3 hoverPos;

	public bool waiting;

	[SerializeField]
	[AssertNotNull]
	private Animator animator;

	[SerializeField]
	[AssertNotNull]
	private Rigidbody rigidbody;

	private bool hadParent;

	private bool updateRestPos;

	private Transform currentBeamPoint;

	public int botId
	{
		get
		{
			return _botId;
		}
		set
		{
			animator.SetInteger(AnimatorHashID.botId, value);
			_botId = value;
		}
	}

	public bool launch
	{
		get
		{
			return _launch;
		}
		set
		{
			if (_launch)
			{
				assignedHoverLoopSound.Play();
			}
			else
			{
				assignedHoverLoopSound.Stop();
			}
			_launch = value;
			animator.SetBool(AnimatorHashID.launch, _launch);
		}
	}

	public bool usingMenu
	{
		get
		{
			return _usingMenu;
		}
		set
		{
			_usingMenu = value;
			animator.SetBool(AnimatorHashID.useing, _usingMenu);
		}
	}

	private bool building
	{
		get
		{
			return _building;
		}
		set
		{
			if (_building != value)
			{
				currentBeamPoint = null;
				if (value)
				{
					CancelInvoke("FindClosestBeamPoint");
					InvokeRepeating("FindClosestBeamPoint", 0f, 0.3f);
				}
				else
				{
					CancelInvoke("FindClosestBeamPoint");
				}
			}
			_building = value;
			animator.SetBool(AnimatorHashID.building, _building);
		}
	}

	private void FindClosestBeamPoint()
	{
		currentBeamPoint = null;
		if (constructObject == null)
		{
			building = false;
			return;
		}
		BuildBotBeamPoints componentInChildren = constructObject.GetComponentInChildren<BuildBotBeamPoints>();
		if (componentInChildren != null)
		{
			currentBeamPoint = componentInChildren.GetClosestTransform(base.transform.position);
		}
	}

	public void SetPath(BuildBotPath newpath, GameObject toConstruct)
	{
		path = newpath;
		atPathIndex = 0;
		hoverPos = path.points[0].position;
		flying = true;
		base.transform.parent = null;
		updateRestPos = false;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, isKinematic: false);
		building = true;
		constructObject = toConstruct;
	}

	private void Start()
	{
		buildbots.Add(this);
		animator.SetInteger(AnimatorHashID.botId, _botId);
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, isKinematic: true);
		assignedHoverLoopSound = hoverLoopSounds[(buildbots.Count - 1) % hoverLoopSounds.Length];
		lineRenderer = base.gameObject.AddComponent<LineRenderer>();
		lineRenderer.useWorldSpace = true;
		lineRenderer.positionCount = 2;
		lineRenderer.startWidth = 0.1f;
		lineRenderer.endWidth = 1f;
		lineRenderer.startColor = new Color(0f, 1f, 1f, 1f);
		lineRenderer.endColor = new Color(1f, 0f, 0f, 1f);
		lineRenderer.material = beamMaterial;
		lineRenderer.enabled = false;
		launch = false;
		building = false;
	}

	private void OnDestroy()
	{
		buildbots.Remove(this);
	}

	public void FinishConstruction()
	{
		constructObject = null;
		path = null;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, isKinematic: true);
		building = false;
	}

	private void Update()
	{
		bool flag = building && currentBeamPoint != null && (currentBeamPoint.transform.position - base.transform.position).magnitude < 8f;
		lineRenderer.enabled = flag;
		if (flag)
		{
			lineRenderer.SetPosition(0, beamOrigin.position);
			lineRenderer.SetPosition(1, currentBeamPoint.transform.position);
			buildLoopingSound.Play();
		}
		else
		{
			buildLoopingSound.Stop();
		}
		UpdateKinematicAnimation();
	}

	private void UpdateKinematicAnimation()
	{
		if (updateRestPos)
		{
			float num = Mathf.Clamp(base.transform.localPosition.magnitude, 0.05f, 3f) * 2f;
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Vector3.zero, Time.deltaTime * num);
			flying = false;
			if (base.transform.localPosition == Vector3.zero)
			{
				updateRestPos = false;
			}
		}
	}

	private void FixedUpdate()
	{
		bool flag = base.transform.parent != null;
		if (hadParent != flag && flag)
		{
			updateRestPos = true;
		}
		if (flying)
		{
			Vector3 value = hoverPos - base.transform.position;
			if (value.sqrMagnitude > 1.6f)
			{
				rigidbody.AddForce(Vector3.Normalize(value) * Time.fixedDeltaTime * 5f, ForceMode.VelocityChange);
			}
			else if ((bool)path)
			{
				atPathIndex++;
				if (atPathIndex >= path.points.Length)
				{
					atPathIndex = 0;
				}
				hoverPos = path.points[atPathIndex].position;
			}
		}
		hadParent = flag;
		if (waiting && !updateRestPos)
		{
			base.transform.localPosition = Vector3.up * Mathf.Sin(Time.time * 4f) * 0.2f;
		}
	}
}
