using System;
using System.Globalization;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class PageRegionsForCopy
	{
		public long Start;

		public long End;

		public PageRegionsForCopy()
		{
		}

		public override string ToString()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] start = new object[] { this.Start, this.End };
			return string.Format(invariantCulture, "Start: {0} End: {1}", start);
		}
	}
}