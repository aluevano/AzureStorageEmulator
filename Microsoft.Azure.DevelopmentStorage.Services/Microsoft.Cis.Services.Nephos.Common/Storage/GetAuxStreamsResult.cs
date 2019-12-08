using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class GetAuxStreamsResult
	{
		public List<string> IndexBlobAuxStreamNames
		{
			get;
			set;
		}

		public List<string> ListBlobAuxStreamNames
		{
			get;
			set;
		}

		public int SequenceNumber
		{
			get;
			set;
		}

		public GetAuxStreamsResult()
		{
			this.ListBlobAuxStreamNames = new List<string>();
			this.IndexBlobAuxStreamNames = new List<string>();
		}
	}
}