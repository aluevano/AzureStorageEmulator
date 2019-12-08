using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class CorsRule : ICloneable
	{
		private bool? allowAllOrigins;

		private bool? allowAllAllowedHeaders;

		private bool? allowAllExposedHeaders;

		public bool AllowAllAllowedHeaders
		{
			get
			{
				if (!this.allowAllAllowedHeaders.HasValue)
				{
					this.allowAllAllowedHeaders = new bool?(this.AllowedPrefixedHeaders.Contains(AnalyticsSettings.WildCard));
				}
				return this.allowAllAllowedHeaders.Value;
			}
		}

		public bool AllowAllExposedHeaders
		{
			get
			{
				if (!this.allowAllExposedHeaders.HasValue)
				{
					this.allowAllExposedHeaders = new bool?(this.ExposedPrefixedHeaders.Contains(AnalyticsSettings.WildCard));
				}
				return this.allowAllExposedHeaders.Value;
			}
		}

		public bool AllowAllOrigins
		{
			get
			{
				if (!this.allowAllOrigins.HasValue)
				{
					this.allowAllOrigins = new bool?(this.AllowedOrigins.Contains(AnalyticsSettings.WildCard));
				}
				return this.allowAllOrigins.Value;
			}
		}

		public HashSet<string> AllowedLiteralHeaders
		{
			get;
			set;
		}

		public HashSet<string> AllowedMethods
		{
			get;
			set;
		}

		public HashSet<string> AllowedOrigins
		{
			get;
			set;
		}

		public List<string> AllowedPrefixedHeaders
		{
			get;
			set;
		}

		public HashSet<string> ExposedLiteralHeaders
		{
			get;
			set;
		}

		public List<string> ExposedPrefixedHeaders
		{
			get;
			set;
		}

		public int MaxAge
		{
			get;
			set;
		}

		public CorsRule()
		{
		}

		public object Clone()
		{
			CorsRule corsRule = new CorsRule()
			{
				AllowedLiteralHeaders = new HashSet<string>(this.AllowedLiteralHeaders),
				AllowedPrefixedHeaders = this.AllowedPrefixedHeaders.ToList<string>(),
				AllowedMethods = new HashSet<string>(this.AllowedMethods),
				ExposedLiteralHeaders = new HashSet<string>(this.ExposedLiteralHeaders),
				ExposedPrefixedHeaders = this.ExposedPrefixedHeaders.ToList<string>(),
				AllowedOrigins = new HashSet<string>(this.AllowedOrigins),
				MaxAge = this.MaxAge
			};
			return corsRule;
		}
	}
}