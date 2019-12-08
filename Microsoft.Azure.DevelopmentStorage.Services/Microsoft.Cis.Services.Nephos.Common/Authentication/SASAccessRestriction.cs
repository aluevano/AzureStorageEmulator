using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class SASAccessRestriction
	{
		public SASAccessLevel AccessLevel
		{
			get;
			private set;
		}

		public NephosUriComponents AccessPath
		{
			get;
			private set;
		}

		public FileSASAccessLevel FileAccessLevel
		{
			get;
			private set;
		}

		public SASAccessRestriction(SASAccessLevel accessLevel, NephosUriComponents accessPath)
		{
			this.AccessLevel = accessLevel;
			this.AccessPath = accessPath;
		}

		public SASAccessRestriction(FileSASAccessLevel accessLevel, NephosUriComponents accessPath)
		{
			this.FileAccessLevel = accessLevel;
			this.AccessPath = accessPath;
		}
	}
}