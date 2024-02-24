using ProtoBuf;

[ProtoContract]
[ProtoInclude(5110, typeof(Bioreactor))]
[ProtoInclude(5120, typeof(NuclearReactor))]
public abstract class PowerCrafter : Crafter
{
}
