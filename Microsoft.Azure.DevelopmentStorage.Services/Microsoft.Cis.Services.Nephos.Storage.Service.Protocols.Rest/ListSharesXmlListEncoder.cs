using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListSharesXmlListEncoder : ListContainersXmlListEncoder
	{
		public ListSharesXmlListEncoder()
		{
			this.shouldEncloseEtagsInQuotes = true;
			this.XmlContainerElementName = "Share";
			this.XmlContainersElementName = "Shares";
		}
	}
}