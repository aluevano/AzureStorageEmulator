using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ListBlobsResult : IListBlobsResultCollection, IEnumerable<IListBlobResultsBlobProperties>, IEnumerable, IDisposable
	{
		private string separator;

		private IBlobObjectCollection blobs;

		private BlobServiceVersion version;

		private IBlobContainer container;

		private bool isDisposed;

		public string NextMarker
		{
			get
			{
				this.CheckDisposed();
				if (!this.blobs.HasMoreRows)
				{
					return string.Empty;
				}
				if (!this.blobs.IsListingByAccount)
				{
					string str = null;
					if (this.version >= BlobServiceVersion.Sept09)
					{
						NephosAssertionException.Assert(!string.IsNullOrEmpty(this.blobs.NextBlobStart), "NextBlobStart must be present.");
						bool hasValue = this.blobs.NextSnapshotStart.HasValue;
						object[] nextBlobStart = new object[] { this.blobs.NextBlobStart };
						NephosAssertionException.Assert(hasValue, "NextSnapshotStart must have a value (NextBlobStart = '{0}').", nextBlobStart);
						IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
						object[] httpString = new object[] { this.blobs.NextBlobStart, null };
						DateTime? nextSnapshotStart = this.blobs.NextSnapshotStart;
						httpString[1] = HttpUtilities.ConvertSnapshotDateTimeToHttpString(nextSnapshotStart.Value);
						verbose.Log("Listing blobs across containers continuation: NextBlobStart = '{0}', NextSnapshotStart = '{1}'.", httpString);
						string[] strArrays = new string[] { this.blobs.NextBlobStart, null };
						DateTime? nullable = this.blobs.NextSnapshotStart;
						strArrays[1] = HttpUtilities.ConvertSnapshotDateTimeToHttpString(nullable.Value);
						str = ContinuationTokenParser.EncodeContinuationTokenV2(strArrays);
					}
					else
					{
						Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Listing blobs within a container: continuation will not include NextSnapshotStart.");
						str = ContinuationTokenParser.EncodeContinuationToken(this.blobs.NextBlobStart);
					}
					return str;
				}
				string str1 = null;
				if (this.version >= BlobServiceVersion.Sept09)
				{
					NephosAssertionException.Assert(!string.IsNullOrEmpty(this.blobs.NextContainerStart), "NextContainerStart must be present.");
					object[] nextContainerStart = new object[] { this.blobs.NextContainerStart };
					NephosAssertionException.Assert(!string.IsNullOrEmpty(this.blobs.NextBlobStart), "NextBlobStart must be present (NextContainerStart = '{0}').", nextContainerStart);
					bool flag = this.blobs.NextSnapshotStart.HasValue;
					object[] objArray = new object[] { this.blobs.NextContainerStart, this.blobs.NextBlobStart };
					NephosAssertionException.Assert(flag, "NextSnapshotStart must have a value (NextContainerStart = '{0}', NextBlobStart = '{1}').", objArray);
					IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] nextContainerStart1 = new object[] { this.blobs.NextContainerStart, this.blobs.NextBlobStart, null };
					DateTime? nextSnapshotStart1 = this.blobs.NextSnapshotStart;
					nextContainerStart1[2] = HttpUtilities.ConvertSnapshotDateTimeToHttpString(nextSnapshotStart1.Value);
					stringDataEventStream.Log("Listing blobs across containers continuation: NextContainerStart = '{0}', NextBlobStart = '{1}', NextSnapshotStart = '{2}'.", nextContainerStart1);
					string[] httpString1 = new string[] { this.blobs.NextContainerStart, this.blobs.NextBlobStart, null };
					DateTime? nullable1 = this.blobs.NextSnapshotStart;
					httpString1[2] = HttpUtilities.ConvertSnapshotDateTimeToHttpString(nullable1.Value);
					str1 = ContinuationTokenParser.EncodeContinuationTokenV2(httpString1);
				}
				else
				{
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Listing blobs across containers: continuation will not include NextSnapshotStart.");
					str1 = ContinuationTokenParser.EncodeContinuationToken(string.Concat(this.blobs.NextContainerStart, "/", this.blobs.NextBlobStart));
				}
				return str1;
			}
		}

		public ListBlobsResult(string separator, IBlobObjectCollection blobs, IBlobContainer container, BlobServiceVersion version)
		{
			if (blobs == null)
			{
				throw new ArgumentNullException("blobs");
			}
			this.blobs = blobs;
			this.separator = separator;
			this.container = container;
			this.version = version;
		}

		private void CheckDisposed()
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException("ListBlobsResult");
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.container != null)
				{
					this.container.Dispose();
					this.container = null;
				}
				if (this.blobs != null)
				{
					this.blobs.Dispose();
				}
			}
			this.isDisposed = true;
		}

		public IEnumerator<IListBlobResultsBlobProperties> GetEnumerator()
		{
			this.CheckDisposed();
			foreach (IBlobObject blobObject in this.blobs)
			{
				using (blobObject)
				{
					yield return new ListBlobResultsBlobProperties(blobObject, this.separator);
				}
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}