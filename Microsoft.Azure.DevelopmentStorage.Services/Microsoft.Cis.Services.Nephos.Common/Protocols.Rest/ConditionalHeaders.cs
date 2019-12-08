using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class ConditionalHeaders
	{
		public string CopySourceIfMatch
		{
			get;
			set;
		}

		public string CopySourceIfModifiedSince
		{
			get;
			set;
		}

		public string CopySourceIfNoneMatch
		{
			get;
			set;
		}

		public string CopySourceIfUnmodifiedSince
		{
			get;
			set;
		}

		public string IfMatch
		{
			get;
			set;
		}

		public string IfModifiedSince
		{
			get;
			set;
		}

		public string IfNoneMatch
		{
			get;
			set;
		}

		public string IfRange
		{
			get;
			set;
		}

		public string IfUnmodifiedSince
		{
			get;
			set;
		}

		public ConditionalHeaders()
		{
		}
	}
}