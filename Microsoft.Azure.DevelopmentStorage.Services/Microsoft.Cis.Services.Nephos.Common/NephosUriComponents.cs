using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class NephosUriComponents
	{
		private string accountName;

		public string AccountName
		{
			get
			{
				return this.accountName;
			}
			set
			{
				this.accountName = value;
				if (this.accountName != null)
				{
					this.AdjustAccountNameIfSecondaryAccess();
				}
			}
		}

		public string ContainerName
		{
			get;
			set;
		}

		public bool IsRemainingPartPresentButEmpty
		{
			get;
			set;
		}

		public bool IsRootContainerImplicit
		{
			get;
			set;
		}

		public bool IsSecondaryAccountAccess
		{
			get;
			set;
		}

		public bool IsUriPathStyle
		{
			get;
			set;
		}

		public string RemainingPart
		{
			get;
			set;
		}

		public NephosUriComponents(string accountName, string containerName, string remainingPart)
		{
			this.AccountName = accountName;
			this.ContainerName = containerName;
			this.RemainingPart = remainingPart;
		}

		public NephosUriComponents(string accountName, string containerName) : this(accountName, containerName, null)
		{
		}

		public NephosUriComponents(string accountName) : this(accountName, null, null)
		{
		}

		public NephosUriComponents()
		{
			this.IsSecondaryAccountAccess = false;
		}

		public void AdjustAccountNameIfSecondaryAccess()
		{
			if (!this.accountName.EndsWith("-secondary", StringComparison.OrdinalIgnoreCase))
			{
				this.IsSecondaryAccountAccess = false;
				return;
			}
			this.accountName = this.accountName.Substring(0, this.accountName.Length - "-secondary".Length);
			this.IsSecondaryAccountAccess = true;
		}

		public void AdjustForRootContainer()
		{
			if (!string.IsNullOrEmpty(this.AccountName) && !string.IsNullOrEmpty(this.ContainerName) && string.IsNullOrEmpty(this.RemainingPart))
			{
				this.RemainingPart = this.ContainerName;
				this.ContainerName = "$root";
				this.IsRootContainerImplicit = true;
			}
		}

		public string GetSecondaryAccountName()
		{
			if (string.IsNullOrEmpty(this.AccountName))
			{
				return this.AccountName;
			}
			return string.Concat(this.AccountName, "-secondary");
		}

		public override string ToString()
		{
			object[] accountName = new object[] { this.AccountName ?? "<null>", this.ContainerName ?? "<null>", this.RemainingPart ?? "<null>", this.IsSecondaryAccountAccess, this.IsUriPathStyle };
			return string.Format("[AccountName = '{0}', ContainerName = '{1}', RemainingPart = '{2}', IsSecondaryAccountAccess = '{3}', IsUriPathStyle = '{4}']", accountName);
		}
	}
}