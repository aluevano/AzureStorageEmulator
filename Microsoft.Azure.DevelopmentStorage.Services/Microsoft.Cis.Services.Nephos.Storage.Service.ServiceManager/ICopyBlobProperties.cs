using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface ICopyBlobProperties
	{
		NameValueCollection BlobMetadata
		{
			get;
		}
	}
}