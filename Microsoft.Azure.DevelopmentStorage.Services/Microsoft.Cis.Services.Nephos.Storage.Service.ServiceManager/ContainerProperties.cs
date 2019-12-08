using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ContainerProperties : IContainerProperties
	{
		private NameValueCollection containerMetadata;

		public ContainerAclSettings ContainerAcl
		{
			get
			{
				return JustDecompileGenerated_get_ContainerAcl();
			}
			set
			{
				JustDecompileGenerated_set_ContainerAcl(value);
			}
		}

		private ContainerAclSettings JustDecompileGenerated_ContainerAcl_k__BackingField;

		public ContainerAclSettings JustDecompileGenerated_get_ContainerAcl()
		{
			return this.JustDecompileGenerated_ContainerAcl_k__BackingField;
		}

		public void JustDecompileGenerated_set_ContainerAcl(ContainerAclSettings value)
		{
			this.JustDecompileGenerated_ContainerAcl_k__BackingField = value;
		}

		public NameValueCollection ContainerMetadata
		{
			get
			{
				return this.containerMetadata;
			}
		}

		public long ContainerQuotaInGB
		{
			get
			{
				return JustDecompileGenerated_get_ContainerQuotaInGB();
			}
			set
			{
				JustDecompileGenerated_set_ContainerQuotaInGB(value);
			}
		}

		private long JustDecompileGenerated_ContainerQuotaInGB_k__BackingField;

		public long JustDecompileGenerated_get_ContainerQuotaInGB()
		{
			return this.JustDecompileGenerated_ContainerQuotaInGB_k__BackingField;
		}

		public void JustDecompileGenerated_set_ContainerQuotaInGB(long value)
		{
			this.JustDecompileGenerated_ContainerQuotaInGB_k__BackingField = value;
		}

		public DateTime LastModifiedTime
		{
			get
			{
				return JustDecompileGenerated_get_LastModifiedTime();
			}
			set
			{
				JustDecompileGenerated_set_LastModifiedTime(value);
			}
		}

		private DateTime JustDecompileGenerated_LastModifiedTime_k__BackingField;

		public DateTime JustDecompileGenerated_get_LastModifiedTime()
		{
			return this.JustDecompileGenerated_LastModifiedTime_k__BackingField;
		}

		public void JustDecompileGenerated_set_LastModifiedTime(DateTime value)
		{
			this.JustDecompileGenerated_LastModifiedTime_k__BackingField = value;
		}

		public ILeaseInfo LeaseInfo
		{
			get
			{
				return JustDecompileGenerated_get_LeaseInfo();
			}
			set
			{
				JustDecompileGenerated_set_LeaseInfo(value);
			}
		}

		private ILeaseInfo JustDecompileGenerated_LeaseInfo_k__BackingField;

		public ILeaseInfo JustDecompileGenerated_get_LeaseInfo()
		{
			return this.JustDecompileGenerated_LeaseInfo_k__BackingField;
		}

		public void JustDecompileGenerated_set_LeaseInfo(ILeaseInfo value)
		{
			this.JustDecompileGenerated_LeaseInfo_k__BackingField = value;
		}

		public ContainerProperties()
		{
			this.containerMetadata = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
		}
	}
}