using UnityEngine;

public class PrecursorPrisonAntechamber_ParamInterconnect : MonoBehaviour
{
	[AssertNotNull]
	public Animator precPrisonAntechamber;

	[AssertNotNull]
	public AnteChamber anteChamber;

	private void upSpinr_spin(int value)
	{
		precPrisonAntechamber.SetBool("upSpinr_spin", value != 0);
	}

	private void upSpinr_oscil(int value)
	{
		precPrisonAntechamber.SetBool("upSpinr_oscilate", value != 0);
	}

	private void upSpinr_extend(int value)
	{
		precPrisonAntechamber.SetBool("upSpinr_extend", value != 0);
	}

	private void upInnrColumn_extend(int value)
	{
		precPrisonAntechamber.SetBool("upInnrColumn_extend", value != 0);
	}

	private void upOutrColumns_extend(int value)
	{
		precPrisonAntechamber.SetBool("upOutrColumns_extend", value != 0);
	}

	private void loOutrColumns_extend(int value)
	{
		precPrisonAntechamber.SetBool("loOutrColumns_extend", value != 0);
	}

	private void loInnrColumn_extend(int value)
	{
		precPrisonAntechamber.SetBool("loInnrColumn_extend", value != 0);
	}

	private void loSpinnr_spin(int value)
	{
		precPrisonAntechamber.SetBool("loSpinnr_spin", value == 1);
		precPrisonAntechamber.SetBool("loSpinnr_counterSpin", value == -1);
	}

	public void StartScanAnimation()
	{
		precPrisonAntechamber.SetBool("upSpinr_spin", value: true);
	}

	public void StopScanAnimation()
	{
		precPrisonAntechamber.SetBool("upSpinr_spin", value: false);
	}

	public void SetPillarRaised(bool raised)
	{
		precPrisonAntechamber.SetBool("loExtend_sequence", raised);
	}

	public void PlayerInRoom(bool inRoom)
	{
		precPrisonAntechamber.SetBool("upprExtend_sequence", inRoom);
	}
}
