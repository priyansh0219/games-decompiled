using Verse.Sound;

namespace Verse
{
	public class SubEffecter_SoundTriggered : SubEffecter
	{
		public SubEffecter_SoundTriggered(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
		{
			def.soundDef.PlayOneShot(new TargetInfo(A.Cell, A.Map));
		}
	}
}
