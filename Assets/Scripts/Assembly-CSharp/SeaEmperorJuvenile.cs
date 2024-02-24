using ProtoBuf;

[ProtoContract]
public class SeaEmperorJuvenile : Creature
{
	public override void Start()
	{
		base.Start();
		SetFriend(Player.main.gameObject);
	}
}
