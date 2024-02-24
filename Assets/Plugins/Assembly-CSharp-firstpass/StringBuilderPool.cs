using System.Text;

public sealed class StringBuilderPool : Pool<StringBuilderPool>
{
	public readonly StringBuilder sb = new StringBuilder(1024);

	protected override void Deinitialize()
	{
		sb.Length = 0;
	}
}
