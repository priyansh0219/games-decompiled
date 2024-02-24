using UWE;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Trail_v2 : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	private static readonly ArrayPool<Vector3> vectorPool = new ArrayPool<Vector3>(12, 1);

	private LineRenderer line;

	private Transform tr;

	private Vector3[] positions;

	private Vector3[] directions;

	private int i;

	private float timeSinceUpdate;

	private Material lineMaterial;

	private float lineSegment;

	private int currentNumberOfPoints;

	private bool allPointsAdded;

	public int numberOfPoints = 10;

	public float updateSpeed = 0.25f;

	public float startWidth = 0.2f;

	public float endWidth = 0.2f;

	public float randWidthFactor;

	public Vector3 windForce = new Vector3(0f, 0f, 0f);

	public bool randomTextureOffset;

	private float randOffset;

	private Color fadeOutColor = new Color(1f, 1f, 1f, 0f);

	private Color startColor;

	private float lerpAmount;

	private Vector3 tempVec;

	private bool isFadingOut;

	public bool isPlaying;

	public bool playBool;

	private float tile;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "Trail_v2";
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
		vectorPool.Return(positions);
		vectorPool.Return(directions);
		Object.Destroy(lineMaterial);
	}

	private void Awake()
	{
		tr = base.transform;
		line = GetComponent<LineRenderer>();
		float num = 1f + Random.Range(0f - randWidthFactor, randWidthFactor);
		line.SetWidth(startWidth * num, endWidth * num);
		lineMaterial = line.material;
		startColor = lineMaterial.GetColor(ShaderPropertyID._Color);
		float value = Random.Range(-1000f, 1000f);
		lineMaterial.SetFloat(ShaderPropertyID._RandPhase, value);
		fadeOutColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
		tile = lineMaterial.mainTextureScale.x;
		if (randomTextureOffset)
		{
			randOffset = Random.value * tile;
		}
		lineSegment = 1f / (float)numberOfPoints;
		positions = vectorPool.Get(numberOfPoints);
		directions = vectorPool.Get(numberOfPoints);
	}

	public void ManagedUpdate()
	{
		if (isFadingOut)
		{
			lerpAmount += Time.deltaTime;
			if (lerpAmount > 1f)
			{
				isPlaying = false;
				isFadingOut = false;
				line.SetVertexCount(0);
			}
			else
			{
				lineMaterial.SetColor(ShaderPropertyID._Color, Color.Lerp(startColor, fadeOutColor, lerpAmount));
			}
		}
		if (isPlaying)
		{
			timeSinceUpdate += Time.deltaTime;
			if (timeSinceUpdate > updateSpeed)
			{
				timeSinceUpdate = 0f;
				if (!allPointsAdded)
				{
					currentNumberOfPoints++;
					line.SetVertexCount(currentNumberOfPoints);
					positions[0] = tr.position;
				}
				if (!allPointsAdded && currentNumberOfPoints == numberOfPoints)
				{
					allPointsAdded = true;
				}
				for (i = currentNumberOfPoints - 1; i > 0; i--)
				{
					tempVec = positions[i - 1];
					positions[i] = tempVec + windForce;
				}
			}
			if (currentNumberOfPoints > 1)
			{
				positions[0] = tr.position;
			}
			lineMaterial.mainTextureOffset = new Vector2(lineSegment * (timeSinceUpdate / updateSpeed) * tile + randOffset, 0f);
		}
		line.SetPositions(positions);
		if (!isPlaying && !isFadingOut)
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	public void Play()
	{
		currentNumberOfPoints = 2;
		lerpAmount = 0f;
		lineMaterial.SetColor(ShaderPropertyID._Color, startColor);
		isPlaying = true;
		isFadingOut = false;
		allPointsAdded = false;
		line.SetVertexCount(currentNumberOfPoints);
		for (i = 0; i < currentNumberOfPoints; i++)
		{
			positions[i] = tr.position;
		}
		line.SetPositions(positions);
		BehaviourUpdateUtils.Register(this);
	}

	public void Stop()
	{
		isFadingOut = true;
	}
}
