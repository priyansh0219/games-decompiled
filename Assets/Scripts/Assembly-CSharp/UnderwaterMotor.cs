using Gendarme;
using UWE;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
public class UnderwaterMotor : PlayerMotor
{
	public Vector3 vel;

	public bool fastSwimMode;

	[AssertNotNull]
	public CapsuleCollider capsulecollider;

	[AssertNotNull]
	public float playerSpeedModifier = 1f;

	private const float stepHeight = 1.85f;

	private const float stepDistance = 0.3f;

	private Vector3 desiredVelocity = new Vector3(0f, 0f, 0f);

	private Vector3 surfaceNormal = new Vector3(0f, 1f, 0f);

	private Vector3 colliderCenter = Vector3.zero;

	private float timeLastJump;

	private Vector3 previousVelocity;

	private float currentPlayerSpeedMultipler = 1f;

	private bool recentlyCollided;

	private const float plasteelTankSpeedReduction = 0.2125f;

	private const float tankSpeedReduction = 0.85f;

	private const float doubleTankSpeedReduction = 1f;

	private const float highCapacityTankSpeedReduction = 1.275f;

	private const float surfaceSpeedMultiplier = 1.3f;

	private const float ventGardenSpeedMultiplier = 0.5f;

	private const float minusSpeedForReinforcedDiveSuit = 1f;

	private const float minSpeed = 2f;

	private const float finsMaxSpeedAdjust = 1.9f;

	private const float ultraGlideFinsMaxSpeedAdjust = 3.2f;

	private const float handsOccupiedMaxSpeedPenalty = 1f;

	private const float equipmentTankSpeedReductionMultiplier = 0.5f;

	private const float speedModifierTransition = 0.3f;

	private float lastCameraOffset;

	public override void SetEnabled(bool enabled)
	{
		if (capsulecollider != null)
		{
			capsulecollider.enabled = enabled;
			if (enabled)
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, isKinematic: false, setCollisionDetectionMode: true);
				rb.detectCollisions = true;
				Vector3 velocity = playerController.velocity;
				rb.velocity = new Vector3(velocity.x, velocity.y * 0.5f, velocity.z);
			}
			else
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, isKinematic: true, setCollisionDetectionMode: true);
				rb.detectCollisions = false;
			}
		}
		jumpPressed = false;
		base.enabled = enabled;
	}

	public override Vector3 GetColliderPosition()
	{
		return capsulecollider.center;
	}

	public override void SetControllerRadius(float radius)
	{
		capsulecollider.radius = radius;
	}

	public override float GetControllerRadius()
	{
		return capsulecollider.radius;
	}

	public override void SetControllerHeight(float height, float cameraOffset)
	{
		if (height != capsulecollider.height || lastCameraOffset != cameraOffset)
		{
			capsulecollider.height = height;
			colliderCenter.y = (0f - capsulecollider.height) * 0.5f - cameraOffset;
			capsulecollider.center = colliderCenter;
			lastCameraOffset = cameraOffset;
		}
	}

	public override float GetControllerHeight()
	{
		return capsulecollider.height;
	}

	public override bool IsSprinting()
	{
		return false;
	}

	private float AlterMaxSpeed(float inMaxSpeed)
	{
		float num = inMaxSpeed;
		Inventory main = Inventory.main;
		Equipment equipment = main.equipment;
		ItemsContainer container = main.container;
		switch (equipment.GetTechTypeInSlot("Tank"))
		{
		case TechType.PlasteelTank:
			num -= 17f / 160f;
			break;
		case TechType.Tank:
			num -= 0.425f;
			break;
		case TechType.DoubleTank:
			num -= 0.5f;
			break;
		case TechType.HighCapacityTank:
			num -= 0.6375f;
			break;
		}
		int count = container.GetCount(TechType.HighCapacityTank);
		num -= (float)count * 1.275f;
		if (num < 2f)
		{
			num = 2f;
		}
		TechType techTypeInSlot = equipment.GetTechTypeInSlot("Body");
		if (techTypeInSlot == TechType.ReinforcedDiveSuit)
		{
			num = Mathf.Max(2f, num - 1f);
		}
		bool flag = Player.main.motorMode == Player.MotorMode.Seaglide;
		if (!flag)
		{
			switch (equipment.GetTechTypeInSlot("Foots"))
			{
			case TechType.Fins:
				num += 1.9f;
				break;
			case TechType.UltraGlideFins:
				num += 3.2f;
				break;
			}
		}
		if (!flag && main.GetHeldTool() != null)
		{
			num -= 1f;
		}
		if (base.gameObject.transform.position.y > Player.main.GetWaterLevel())
		{
			num *= 1.3f;
		}
		currentPlayerSpeedMultipler = Mathf.MoveTowards(currentPlayerSpeedMultipler, playerSpeedModifier, 0.3f * Time.deltaTime);
		return num * currentPlayerSpeedMultipler;
	}

	public override void SetVelocity(Vector3 velocity)
	{
		if (!fastSwimMode)
		{
			rb.velocity = velocity;
		}
	}

	public override Vector3 UpdateMove()
	{
		Rigidbody rigidbody = rb;
		if (playerController == null || playerController.forwardReference == null)
		{
			return rigidbody.velocity;
		}
		fastSwimMode = Player.main.debugFastSwimAllowed && GameInput.GetButtonHeld(GameInput.Button.Sprint);
		Vector3 velocity = rigidbody.velocity;
		Vector3 vector = movementInputDirection;
		float y = vector.y;
		float num = Mathf.Min(1f, vector.magnitude);
		vector.y = 0f;
		vector.Normalize();
		float a = 0f;
		if (vector.z > 0f)
		{
			a = forwardMaxSpeed;
		}
		else if (vector.z < 0f)
		{
			a = backwardMaxSpeed;
		}
		if (vector.x != 0f)
		{
			a = Mathf.Max(a, strafeMaxSpeed);
		}
		a = Mathf.Max(a, verticalMaxSpeed);
		float num2 = a;
		a = AlterMaxSpeed(num2);
		float num3 = a / num2;
		a *= playerController.player.mesmerizedSpeedMultiplier;
		if (fastSwimMode)
		{
			a *= 1000f;
		}
		float num4 = Mathf.Max(b: a * debugSpeedMult, a: velocity.magnitude);
		Vector3 vector2 = playerController.forwardReference.rotation * vector;
		vector = vector2;
		vector.y += y;
		vector.Normalize();
		if (!canSwim)
		{
			vector.y = 0f;
			vector.Normalize();
		}
		float num5 = airAcceleration;
		if (grounded)
		{
			num5 = groundAcceleration;
		}
		else if (underWater)
		{
			num5 = waterAcceleration;
			num5 *= playerSpeedModifier;
		}
		float num6 = num * num5 * Time.deltaTime;
		if (num3 > 1f)
		{
			num6 *= num3;
		}
		if (num6 > 0f)
		{
			Vector3 vector3 = velocity + vector * num6;
			if (vector3.magnitude > num4)
			{
				vector3.Normalize();
				vector3 *= num4;
			}
			float num7 = Vector3.Dot(vector3, surfaceNormal);
			if (!canSwim)
			{
				vector3 -= num7 * surfaceNormal;
			}
			bool flag = y < 0f;
			bool flag2 = vector2.y < -0.3f;
			float waterLevel = Player.main.GetWaterLevel();
			float num8 = waterLevel + 0.8f - 0.28f;
			float num9 = waterLevel - 0.5f;
			if (base.transform.position.y >= num9 && !flag && !flag2)
			{
				float num10 = Mathf.Pow(Mathf.Clamp01((num8 - base.transform.position.y) / (num8 - num9)), 0.3f);
				vector3.y *= num10;
			}
			Vector3 vector4 = new Vector3(vector.x, 0f, vector.z) * 0.3f;
			if (base.transform.position.y >= waterLevel && (recentlyCollided || TestWillCollide(vector4)) && !Player.main.playerController.Trace(base.transform.position, base.transform.position + Vector3.up * 1.85f, out var hit))
			{
				Vector3 position = base.transform.position;
				Vector3 vector5 = base.transform.position + Vector3.up * 1.85f;
				if (!Player.main.playerController.Trace(vector5, vector5 + vector4, out hit) && Player.main.playerController.Trace(vector5 + vector4, vector5 + vector4 + Vector3.down * 1.85f, out hit) && PlayerMotor.IsWalkable(hit.normal))
				{
					base.transform.position = vector5 + vector4 + Vector3.down * Mathf.Max(0f, hit.distance - 0.1f);
					vector3.y = 0.2f;
					MainCameraControl.main.stepAmount += base.transform.position.y - position.y;
				}
			}
			rigidbody.velocity = vector3;
			desiredVelocity = vector3;
		}
		else
		{
			desiredVelocity = rigidbody.velocity;
		}
		float num11 = (underWater ? underWaterGravity : gravity);
		if (num11 != 0f)
		{
			rigidbody.AddForce(new Vector3(0f, (0f - num11) * Time.deltaTime, 0f), ForceMode.VelocityChange);
			usingGravity = true;
		}
		else
		{
			usingGravity = false;
		}
		float drag = airDrag;
		if (underWater)
		{
			drag = swimDrag;
		}
		else if (grounded)
		{
			drag = groundDrag;
		}
		rigidbody.drag = drag;
		if (fastSwimMode)
		{
			rigidbody.drag = 0f;
		}
		grounded = false;
		vel = rigidbody.velocity;
		recentlyCollided = false;
		return vel;
	}

	private bool TestWillCollide(Vector3 deltaMove)
	{
		RaycastHit hitInfo;
		return rb.SweepTest(deltaMove.normalized, out hitInfo, deltaMove.magnitude, QueryTriggerInteraction.Ignore);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Rigidbody rigidbody = rb;
		MovementCollisionData movementCollisionData = default(MovementCollisionData);
		movementCollisionData.impactVelocity = rigidbody.velocity - previousVelocity;
		VFXSurfaceTypes vFXSurfaceTypes = Utils.GetObjectSurfaceType(collision.gameObject);
		if (vFXSurfaceTypes == VFXSurfaceTypes.none)
		{
			vFXSurfaceTypes = Utils.GetTerrainSurfaceType(collision.contacts[0].point, collision.contacts[0].normal);
		}
		movementCollisionData.surfaceType = vFXSurfaceTypes;
		SendMessage("OnMovementCollision", movementCollisionData, SendMessageOptions.DontRequireReceiver);
		previousVelocity = rigidbody.velocity;
		recentlyCollided = true;
	}

	private void OnCollisionStay(Collision collision)
	{
		_ = grounded;
		grounded = false;
		Vector3 vector = default(Vector3);
		int num = 0;
		for (int i = 0; i < collision.contacts.Length; i++)
		{
			ContactPoint contactPoint = collision.contacts[i];
			if (num == 0)
			{
				vector = contactPoint.normal;
			}
			else
			{
				vector += contactPoint.normal;
			}
			num++;
		}
		if (num > 0)
		{
			vector /= (float)num;
			grounded = true;
			if (vector.y > 0.5f)
			{
				grounded = true;
			}
			surfaceNormal = vector;
		}
		else
		{
			surfaceNormal = new Vector3(0f, 1f, 0f);
		}
		recentlyCollided = true;
	}

	public override Collider GetCollider()
	{
		return capsulecollider;
	}
}
