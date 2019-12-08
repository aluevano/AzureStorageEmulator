namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IUpdateOptions
	{
		MetadataOption ApplicationMetadataOption
		{
			get;
		}

		MetadataOption ServiceMetadataOption
		{
			get;
		}
	}
}