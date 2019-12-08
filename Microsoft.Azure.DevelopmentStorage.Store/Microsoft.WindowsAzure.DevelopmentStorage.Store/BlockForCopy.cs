using System;
using System.Globalization;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class BlockForCopy
	{
		public string BlockId;

		public int Length;

		public BlockForCopy()
		{
		}

		public override string ToString()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] blockId = new object[] { this.BlockId, this.Length };
			return string.Format(invariantCulture, "BlockId: {0} Length: {1}", blockId);
		}
	}
}