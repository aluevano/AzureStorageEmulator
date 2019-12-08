using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ArchiveBlobContext
	{
		public string GenerationId
		{
			get;
			set;
		}

		public bool IsOperationAllowedOnArchivedBlobs
		{
			get;
			set;
		}

		public DateTime? LMT
		{
			get;
			set;
		}

		public Guid RequestId
		{
			get;
			set;
		}

		public ArchiveBlobContext()
		{
		}
	}
}