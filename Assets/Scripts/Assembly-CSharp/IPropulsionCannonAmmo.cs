public interface IPropulsionCannonAmmo
{
	void OnGrab();

	void OnShoot();

	void OnRelease();

	void OnImpact();

	bool GetAllowedToGrab();

	bool GetAllowedToShoot();
}
