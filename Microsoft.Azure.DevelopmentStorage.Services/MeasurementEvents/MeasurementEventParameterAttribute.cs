using System;

namespace MeasurementEvents
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	public sealed class MeasurementEventParameterAttribute : Attribute
	{
		private readonly string paramNameOverride;

		public string ParameterNameOverride
		{
			get
			{
				return this.paramNameOverride;
			}
		}

		public MeasurementEventParameterAttribute()
		{
		}

		public MeasurementEventParameterAttribute(string parameterNameOverride)
		{
			this.paramNameOverride = parameterNameOverride;
		}
	}
}