using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class ConversionExtensions
	{
		public static string ToBase64String(this long? value)
		{
			if (!value.HasValue)
			{
				return null;
			}
			return Convert.ToBase64String(BitConverter.GetBytes(value.Value));
		}
	}
}