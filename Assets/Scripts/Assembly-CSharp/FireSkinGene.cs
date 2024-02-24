public class FireSkinGene : Gene
{
	private void Start()
	{
		onChangedEvent.AddHandler(base.gameObject, OnChanged);
	}

	private void OnChanged(float newScalar)
	{
	}
}
