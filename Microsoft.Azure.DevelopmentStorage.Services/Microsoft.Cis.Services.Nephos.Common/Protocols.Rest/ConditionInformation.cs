using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class ConditionInformation
	{
		public HttpStatusCode ConditionFailStatusCode
		{
			get;
			set;
		}

		public DateTime? CopySourceIfMatch
		{
			get;
			set;
		}

		public DateTime? CopySourceIfModifiedSince
		{
			get;
			set;
		}

		public DateTime? CopySourceIfNoneMatch
		{
			get;
			set;
		}

		public DateTime? CopySourceIfNotModifiedSince
		{
			get;
			set;
		}

		public DateTime[] IfMatch
		{
			get;
			set;
		}

		public DateTime? IfModifiedSince
		{
			get;
			set;
		}

		public DateTime[] IfNoneMatch
		{
			get;
			set;
		}

		public DateTime? IfNotModifiedSince
		{
			get;
			set;
		}

		public bool IsMultipleConditionalHeaderEnabled
		{
			get;
			set;
		}

		public ResourceExistenceCondition? ResourceExistsCondition
		{
			get;
			set;
		}

		public ConditionInformation()
		{
		}
	}
}