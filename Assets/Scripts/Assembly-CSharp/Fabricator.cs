using FMODUnity;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Fabricator : GhostCrafter
{
	public Animator animator;

	[AssertNotNull]
	public Transform soundOrigin;

	[AssertNotNull]
	public FMODAsset openSound;

	[AssertNotNull]
	public FMODAsset closeSound;

	[AssertNotNull]
	public StudioEventEmitter fabricateSound;

	public GameObject sparksPS;

	public GameObject leftBeam;

	public GameObject rightBeam;

	public Light fabLight;

	private GameObject sparksR;

	private GameObject sparksL;

	private float beamsDelay;

	protected override void Start()
	{
		base.Start();
		Transform obj = ((ghost != null && ghost.itemSpawnPoint != null) ? ghost.itemSpawnPoint : base.transform);
		Vector3 position = obj.position;
		Vector3 up = obj.up;
		sparksL = Utils.SpawnPrefabAt(sparksPS, leftBeam.transform, GetBeamEnd(leftBeam.transform.position, leftBeam.transform.forward, position, up));
		sparksR = Utils.SpawnPrefabAt(sparksPS, rightBeam.transform, GetBeamEnd(rightBeam.transform.position, rightBeam.transform.forward, position, up));
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (base.logic != null && base.logic.inProgress)
		{
			Transform transform = ((ghost != null && ghost.itemSpawnPoint != null) ? ghost.itemSpawnPoint : base.transform);
			Vector3 position = transform.position;
			if (position.y > leftBeam.transform.position.y)
			{
				position.y = leftBeam.transform.position.y;
			}
			Shader.SetGlobalFloat(ShaderPropertyID._FabricatorPosY, position.y + 0.03f);
			beamsDelay += Time.deltaTime;
			if (!(beamsDelay > spawnAnimationDelay))
			{
				return;
			}
			if (!leftBeam.GetComponent<Renderer>().enabled)
			{
				leftBeam.GetComponent<Renderer>().enabled = true;
				rightBeam.GetComponent<Renderer>().enabled = true;
				if (MiscSettings.flashes)
				{
					sparksR.GetComponent<ParticleSystem>().Play();
					sparksL.GetComponent<ParticleSystem>().Play();
				}
				fabLight.enabled = true;
				fabLight.GetComponent<LightAnimator>().enabled = true;
			}
			if (sparksL != null && sparksR != null)
			{
				Vector3 up = transform.up;
				sparksR.transform.position = GetBeamEnd(rightBeam.transform.position, rightBeam.transform.forward, position, up);
				sparksL.transform.position = GetBeamEnd(leftBeam.transform.position, leftBeam.transform.forward, position, up);
			}
		}
		else if (leftBeam.GetComponent<Renderer>().enabled)
		{
			beamsDelay = 0f;
			leftBeam.GetComponent<Renderer>().enabled = false;
			rightBeam.GetComponent<Renderer>().enabled = false;
			sparksR.GetComponent<ParticleSystem>().Stop();
			sparksL.GetComponent<ParticleSystem>().Stop();
			fabLight.GetComponent<LightAnimator>().enabled = false;
			fabLight.enabled = false;
		}
	}

	protected override void OnStateChanged(bool crafting)
	{
		if (animator != null)
		{
			animator.SetBool(AnimatorHashID.fabricating, crafting);
		}
		if (fabricateSound.IsPlaying() != crafting)
		{
			if (crafting)
			{
				fabricateSound.Play();
			}
			else
			{
				fabricateSound.Stop();
			}
		}
	}

	protected override void OnOpenedChanged(bool opened)
	{
		base.OnOpenedChanged(opened);
		if (opened && Inventory.main.GetTotalItemCount() > 0)
		{
			GoalManager.main.OnCustomGoalEvent("Use_Fabricator_Loot");
		}
		if (animator != null)
		{
			animator.SetBool(AnimatorHashID.open_fabricator, opened);
		}
		FMODUWE.PlayOneShot(opened ? openSound : closeSound, soundOrigin.position);
	}

	private static Vector3 GetBeamEnd(Vector3 beamPos, Vector3 beamRot, Vector3 basePos, Vector3 baseRot)
	{
		return beamPos + Vector3.Normalize(beamRot) * (Vector3.Dot(basePos - beamPos, baseRot) / Vector3.Dot(beamRot, baseRot));
	}
}
