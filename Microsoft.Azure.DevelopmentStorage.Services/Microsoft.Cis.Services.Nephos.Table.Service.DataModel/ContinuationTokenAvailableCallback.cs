using System;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Table.Service.DataModel
{
	public delegate void ContinuationTokenAvailableCallback(Dictionary<string, string> continuationToken);
}