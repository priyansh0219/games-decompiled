using System;
using System.IO;
using System.Reflection;
using LitJson;

public static class LitJsonExtensions
{
	private static FieldInfo writerIndentation = typeof(JsonWriter).GetField("indentation", BindingFlags.Instance | BindingFlags.NonPublic);

	private static MethodInfo writerPutNewline = typeof(JsonWriter).GetMethod("PutNewline", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);

	private static readonly FieldInfo writerContext = typeof(JsonWriter).GetField("context", BindingFlags.Instance | BindingFlags.NonPublic);

	private static FieldInfo writerContextCount = writerContext.FieldType.GetField("Count", BindingFlags.Instance | BindingFlags.Public);

	public static void WriteComment(this JsonWriter writer, string comment, Action callback)
	{
		if (writer.PrettyPrint)
		{
			TextWriter textWriter = writer.TextWriter;
			object value = writerContext.GetValue(writer);
			int num = (int)writerContextCount.GetValue(value);
			writerContextCount.SetValue(value, num + 1);
			writerPutNewline.Invoke(writer, null);
			int count = (int)writerIndentation.GetValue(writer);
			textWriter.Write(new string(' ', count));
			textWriter.Write("// ");
			textWriter.Write(comment);
			writerContextCount.SetValue(value, 0);
			callback();
			writerContextCount.SetValue(value, num + 1);
		}
		else
		{
			callback();
		}
	}
}
