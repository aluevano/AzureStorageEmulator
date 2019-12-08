using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
	public sealed class ServiceEntryClassAttribute : Attribute
	{
		public ServiceEntryClassAttribute()
		{
		}
	}
}