using System;

public class Util
{
	/// <summary>Tries to parse given text as given enum</summary>
	public static T ParseEnum<T>(string text)
	{
		return (T)Enum.Parse(typeof(T), text);
	}
}
