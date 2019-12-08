using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class UpdateOptions : IUpdateOptions
	{
		public MetadataOption ApplicationMetadataOption
		{
			get
			{
				return JustDecompileGenerated_get_ApplicationMetadataOption();
			}
			set
			{
				JustDecompileGenerated_set_ApplicationMetadataOption(value);
			}
		}

		private MetadataOption JustDecompileGenerated_ApplicationMetadataOption_k__BackingField;

		public MetadataOption JustDecompileGenerated_get_ApplicationMetadataOption()
		{
			return this.JustDecompileGenerated_ApplicationMetadataOption_k__BackingField;
		}

		private void JustDecompileGenerated_set_ApplicationMetadataOption(MetadataOption value)
		{
			this.JustDecompileGenerated_ApplicationMetadataOption_k__BackingField = value;
		}

		public MetadataOption ServiceMetadataOption
		{
			get
			{
				return JustDecompileGenerated_get_ServiceMetadataOption();
			}
			set
			{
				JustDecompileGenerated_set_ServiceMetadataOption(value);
			}
		}

		private MetadataOption JustDecompileGenerated_ServiceMetadataOption_k__BackingField;

		public MetadataOption JustDecompileGenerated_get_ServiceMetadataOption()
		{
			return this.JustDecompileGenerated_ServiceMetadataOption_k__BackingField;
		}

		private void JustDecompileGenerated_set_ServiceMetadataOption(MetadataOption value)
		{
			this.JustDecompileGenerated_ServiceMetadataOption_k__BackingField = value;
		}

		public UpdateOptions(MetadataOption serviceMetadataOption, MetadataOption applicationMetadataOption)
		{
			this.ServiceMetadataOption = serviceMetadataOption;
			this.ApplicationMetadataOption = applicationMetadataOption;
		}
	}
}