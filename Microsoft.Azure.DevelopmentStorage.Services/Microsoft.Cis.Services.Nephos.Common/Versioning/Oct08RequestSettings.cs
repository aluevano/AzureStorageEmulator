using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Oct08RequestSettings : VersionedRequestSettings
	{
		public Oct08RequestSettings()
		{
			base.VersionString = "2008-10-27";
			base.GetBlobApiEnabled = true;
			base.PutBlobApiEnabled = true;
			base.DeleteBlobApiEnabled = true;
			base.GetBlobPropertiesApiEnabled = true;
			base.GetBlobMetadataApiEnabled = true;
			base.SetBlobMetadataApiEnabled = true;
			base.PutBlockApiEnabled = true;
			base.GetBlockListApiEnabled = true;
			base.PutBlockListApiEnabled = true;
			base.GetContainerPropertiesApiEnabled = true;
			base.CreateContainerApiEnabled = true;
			base.DeleteContainerApiEnabled = true;
			base.ListBlobsApiEnabled = true;
			base.GetContainerMetadataApiEnabled = true;
			base.SetContainerMetadataApiEnabled = true;
			base.GetContainerAclApiEnabled = true;
			base.SetContainerAclApiEnabled = true;
			base.ListContainersApiEnabled = true;
			base.BlobPreflightApiEnabled = true;
			base.ListQueuesApiEnabled = true;
			base.CreateQueueApiEnabled = true;
			base.DeleteQueueApiEnabled = true;
			base.GetQueueMetadataApiEnabled = true;
			base.SetQueueMetadataApiEnabled = true;
			base.GetMessagesApiEnabled = true;
			base.PutMessageApiEnabled = true;
			base.ClearMessagesApiEnabled = true;
			base.DeleteMessageApiEnabled = true;
			base.QueuePreflightApiEnabled = true;
			base.TableAndRowOperationApisEnabled = true;
			base.TablePreflightApiEnabled = true;
		}
	}
}