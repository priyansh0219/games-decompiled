public class HeatBlade : Knife
{
	public FMOD_CustomEmitter idleClip;

	public VFXController fxControl;

	public override void OnDraw(Player p)
	{
		idleClip.Play();
		fxControl.Play();
		base.OnDraw(p);
	}

	public override void OnHolster()
	{
		idleClip.Stop();
		fxControl.StopAndDestroy(0f);
		base.OnHolster();
	}

	protected override int GetUsesPerHit()
	{
		return 3;
	}
}
