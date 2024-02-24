using UnityEngine;

public class SeaTreaderSounds : MonoBehaviour
{
	public SeaTreader treader;

	public AnimationClip walkinAnimClip;

	public Transform mouth;

	public Transform leftLeg;

	public Transform rightLeg;

	public Transform frontLeg;

	public Transform butt;

	public GameObject stepEffect;

	public GameObject stompEffect;

	public FMOD_StudioEventEmitter stepSound;

	public FMOD_StudioEventEmitter startPoopingSound;

	public FMOD_StudioEventEmitter poopSound;

	public FMOD_StudioEventEmitter stompSound;

	public FMOD_StudioEventEmitter attackSound;

	public FMOD_StudioEventEmitter attackDownSound;

	public GameObject stepChunkPrefab;

	public int minChunksNum;

	public int maxChunksNum = 3;

	public float chunkSpawnOffset = 0.7f;

	private float lastStompAttackTime;

	private void SpawnChunks(Transform legTr)
	{
		int num = Random.Range(minChunksNum, maxChunksNum + 1);
		if (!(stepChunkPrefab == null) && num > 0 && Physics.Raycast(legTr.position + legTr.up * 2f, -legTr.up, out var hitInfo, 4f, Voxeland.GetTerrainLayerMask()))
		{
			for (int i = 0; i < num; i++)
			{
				Transform transform = Object.Instantiate(stepChunkPrefab).transform;
				transform.position = hitInfo.point;
				transform.rotation = Random.rotation;
				transform.rotation = Quaternion.FromToRotation(transform.up, hitInfo.normal) * transform.rotation;
				Vector2 vector = Random.insideUnitCircle.normalized * chunkSpawnOffset;
				Vector3 position = new Vector3(vector.x, 0f, vector.y);
				transform.position = transform.TransformPoint(position);
			}
		}
	}

	private void OnStep(Transform legTr, AnimationEvent animationEvent)
	{
		if (!(animationEvent.animatorClipInfo.clip == walkinAnimClip) || treader.IsWalking())
		{
			if (stepEffect != null)
			{
				Utils.SpawnPrefabAt(stepEffect, null, legTr.position);
			}
			if (stepSound != null)
			{
				Utils.PlayEnvSound(stepSound, legTr.position);
			}
			SpawnChunks(legTr);
		}
	}

	public void OnLeftLegStep(AnimationEvent animationEvent)
	{
		OnStep(leftLeg, animationEvent);
	}

	public void OnRightLegStep(AnimationEvent animationEvent)
	{
		OnStep(rightLeg, animationEvent);
	}

	public void OnFrontLegStep(AnimationEvent animationEvent)
	{
		OnStep(frontLeg, animationEvent);
	}

	public void OnStartPooping()
	{
		if (startPoopingSound != null)
		{
			Utils.PlayEnvSound(startPoopingSound, mouth.position);
		}
	}

	public void OnPoop()
	{
		if (poopSound != null)
		{
			Utils.PlayEnvSound(poopSound, butt.position);
		}
	}

	public void OnStomp()
	{
		if (!(Time.time < lastStompAttackTime + 0.2f))
		{
			lastStompAttackTime = Time.time;
			if (stompEffect != null)
			{
				Utils.SpawnPrefabAt(stompEffect, null, frontLeg.position);
			}
			if (stompSound != null)
			{
				Utils.PlayEnvSound(stompSound, frontLeg.position);
			}
			SpawnChunks(frontLeg);
		}
	}

	public void OnAttack()
	{
		if (attackSound != null)
		{
			Utils.PlayEnvSound(attackSound, mouth.position);
		}
	}

	public void OnAttackDown()
	{
		if (attackDownSound != null)
		{
			Utils.PlayEnvSound(attackDownSound, mouth.position);
		}
	}
}
