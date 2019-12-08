using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class BlobHttpDownloader
	{
		private const long _maxChunkLength = 4194304L;

		private string _sourceUrl;

		private bool _isAzureBlob;

		private string _etag;

		private static Dictionary<int, string> httpErrorCodesMap;

		private bool _isBlockBlob;

		private long _contentLength;

		private int _maxNumOfRetries = 4;

		private BlockForCopy[] _blockList;

		private int _currentBlockIndex;

		private long _currentOffset;

		private int _httpErrorCode = 200;

		private string _httpDescription;

		private PageRegionsForCopy[] _pages;

		private int _currentPageIndex;

		private long _peekedBytes;

		private long _currentChunkIndex;

		private long _endChunkIndex;

		private long _pageRegionFetchRange = (long)104857600;

		public int ErrorCode
		{
			get
			{
				return this._httpErrorCode;
			}
		}

		public string ErrorDescription
		{
			get
			{
				return this._httpDescription;
			}
		}

		public bool IsBlockBlob
		{
			get
			{
				return this._isBlockBlob;
			}
		}

		static BlobHttpDownloader()
		{
			BlobHttpDownloader.httpErrorCodesMap = new Dictionary<int, string>()
			{
				{ 304, "NotModified" },
				{ 305, "UseProxy" },
				{ 306, "SwitchProxy" },
				{ 307, "TemporaryRedirect" },
				{ 308, "PermanentRedirect" },
				{ 400, "BadRequest" },
				{ 401, "Unauthorized" },
				{ 402, "PaymentRequired" },
				{ 403, "Forbidden" },
				{ 404, "NotFound" },
				{ 405, "MethodNotAllowed" },
				{ 406, "NotAcceptable" },
				{ 407, "ProxyAuthentication Required" },
				{ 408, "RequestTimeout" },
				{ 409, "Conflict" },
				{ 410, "Gone" },
				{ 411, "LengthRequired" },
				{ 412, "PreconditionFailed" },
				{ 413, "RequestEntityTooLarge" },
				{ 414, "RequestURITooLong" },
				{ 415, "UnsupportedMediaType" },
				{ 416, "RequestedRangeNotSatisfiable" },
				{ 417, "ExpectationFailed" },
				{ 428, "PreconditionRequired" },
				{ 429, "TooManyRequests" },
				{ 450, "BlockedbyWindowsParentalControls" },
				{ 500, "InternalServerError" },
				{ 501, "NotImplemented" },
				{ 502, "BadGateway" },
				{ 503, "ServiceUnavailable" },
				{ 504, "GatewayTimeout" },
				{ 505, "HTTPVersionNotSupported" },
				{ 509, "BandwidthLimitExceeded" }
			};
		}

		public BlobHttpDownloader(string url, bool isAzureBlob, string etag, bool isBlockBlob, long contentLength)
		{
			this._sourceUrl = url;
			this._isAzureBlob = isAzureBlob;
			this._etag = etag;
			this._isBlockBlob = isBlockBlob;
			this._contentLength = contentLength;
			this.PopulateBlocks();
		}

		private void GetBlockList()
		{
			bool flag = false;
			try
			{
				for (int i = 0; i < this._maxNumOfRetries; i++)
				{
					try
					{
						HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create((this._sourceUrl.Contains("?") ? string.Concat(this._sourceUrl, "&comp=blocklist&&blocklisttype=committed") : string.Concat(this._sourceUrl, "?comp=blocklist&&blocklisttype=committed")));
						httpWebRequest.Method = "GET";
						httpWebRequest.ContentLength = (long)0;
						httpWebRequest.Headers["x-ms-version"] = "2011-08-18";
						httpWebRequest.Headers["If-Match"] = this._etag;
						using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
						{
							if (response.StatusCode == HttpStatusCode.OK)
							{
								using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
								{
									List<BlockForCopy> blockForCopies = new List<BlockForCopy>();
									foreach (XElement xElement in XElement.Parse(streamReader.ReadToEnd()).Elements())
									{
										foreach (XElement xElement1 in xElement.Elements("Block"))
										{
											BlockForCopy blockForCopy = new BlockForCopy()
											{
												BlockId = DbListBlobObject.ToHexString(Convert.FromBase64String(xElement1.Element("Name").Value)),
												Length = int.Parse(xElement1.Element("Size").Value)
											};
											blockForCopies.Add(blockForCopy);
										}
									}
									this._blockList = blockForCopies.ToArray<BlockForCopy>();
								}
								if (this._blockList == null || (int)this._blockList.Length < 1)
								{
									this._blockList = new BlockForCopy[1];
									BlockForCopy[] blockForCopyArray = this._blockList;
									BlockForCopy hexString = new BlockForCopy();
									Guid guid = Guid.NewGuid();
									hexString.BlockId = DbListBlobObject.ToHexString(guid.ToByteArray());
									hexString.Length = (int)this._contentLength;
									blockForCopyArray[0] = hexString;
								}
							}
							break;
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						if (i == this._maxNumOfRetries + 1)
						{
							flag = true;
							throw;
						}
						if (!this.IsRetryableException(exception))
						{
							throw;
						}
					}
					Thread.Sleep(1000);
				}
			}
			catch (Exception exception2)
			{
				this.HandleExceptionAndSetStatus(exception2, flag);
			}
		}

		public bool GetNextBlock(out string blockId, out byte[] data)
		{
			bool flag;
			blockId = null;
			data = null;
			if (this._blockList == null || this._currentBlockIndex >= (int)this._blockList.Length)
			{
				return false;
			}
			bool flag1 = false;
			try
			{
				for (int i = 0; i < this._maxNumOfRetries; i++)
				{
					try
					{
						HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this._sourceUrl);
						httpWebRequest.Method = "GET";
						httpWebRequest.ContentLength = (long)0;
						httpWebRequest.Headers["x-ms-version"] = "2011-08-18";
						httpWebRequest.Headers["If-Match"] = this._etag;
						httpWebRequest.AddRange(this._currentOffset, this._currentOffset + (long)this._blockList[this._currentBlockIndex].Length - (long)1);
						using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
						{
							data = new byte[this._blockList[this._currentBlockIndex].Length];
							this.ReadAllBytes(data, response);
							IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
							object[] str = new object[] { this._blockList[this._currentBlockIndex].ToString() };
							info.Log("Downloaded block:{0}", str);
						}
						break;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						if (i == this._maxNumOfRetries + 1)
						{
							flag1 = true;
							throw;
						}
						if (!this.IsRetryableException(exception))
						{
							throw;
						}
					}
					Thread.Sleep(1000);
				}
				blockId = this._blockList[this._currentBlockIndex].BlockId;
				this._currentOffset += (long)this._blockList[this._currentBlockIndex].Length;
				this._currentBlockIndex++;
				return true;
			}
			catch (Exception exception2)
			{
				this.HandleExceptionAndSetStatus(exception2, flag1);
				flag = false;
			}
			return flag;
		}

		private bool GetNextChunk(out byte[] data, out long pageStart)
		{
			bool flag;
			data = null;
			pageStart = (long)0;
			long num = (this._currentChunkIndex == (long)0 ? this._pages[this._currentPageIndex].Start : this._pages[this._currentPageIndex].Start + this._currentChunkIndex * (long)4194304);
			long end = this._pages[this._currentPageIndex].End;
			if (this._currentChunkIndex != this._endChunkIndex)
			{
				end = num + (long)4194304 - (long)1;
			}
			bool flag1 = false;
			try
			{
				for (int i = 0; i < this._maxNumOfRetries; i++)
				{
					try
					{
						HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this._sourceUrl);
						httpWebRequest.Method = "GET";
						httpWebRequest.ContentLength = (long)0;
						httpWebRequest.Headers["x-ms-version"] = "2011-08-18";
						httpWebRequest.Headers["If-Match"] = this._etag;
						httpWebRequest.AddRange(num, end);
						using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
						{
							data = new byte[checked((IntPtr)(end - num + (long)1))];
							this.ReadAllBytes(data, response);
							pageStart = num;
							IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
							object[] objArray = new object[] { num, end };
							info.Log("Downloaded PageChunk:{0}-{1}", objArray);
						}
						break;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						if (i == this._maxNumOfRetries + 1)
						{
							flag1 = true;
							throw;
						}
						if (!this.IsRetryableException(exception))
						{
							throw;
						}
					}
					Thread.Sleep(1000);
				}
				this._currentChunkIndex += (long)1;
				return true;
			}
			catch (Exception exception2)
			{
				this.HandleExceptionAndSetStatus(exception2, flag1);
				flag = false;
			}
			return flag;
		}

		public bool GetNextPage(out byte[] data, out long pageStart)
		{
			data = null;
			pageStart = (long)0;
			if (this._peekedBytes >= this._contentLength && this._pages == null)
			{
				return false;
			}
			if (this._pages == null || this._currentPageIndex >= (int)this._pages.Length)
			{
				this.GetNextPageRanges();
				if (this._httpErrorCode != 200)
				{
					return false;
				}
				if (this._pages == null)
				{
					return this.GetNextPage(out data, out pageStart);
				}
				this.MakeChunks();
			}
			if (this._currentChunkIndex > this._endChunkIndex)
			{
				this._currentPageIndex++;
				if (this._currentPageIndex >= (int)this._pages.Length)
				{
					this._pages = null;
					return this.GetNextPage(out data, out pageStart);
				}
				this.MakeChunks();
			}
			return this.GetNextChunk(out data, out pageStart);
		}

		private void GetNextPageRanges()
		{
			this._pages = null;
			this._currentPageIndex = 0;
			long num = this._peekedBytes;
			long num1 = this._peekedBytes + this._pageRegionFetchRange - (long)1;
			if (num1 >= this._contentLength)
			{
				num1 = this._contentLength - (long)1;
			}
			if (num1 < num)
			{
				return;
			}
			bool flag = false;
			try
			{
				for (int i = 0; i < this._maxNumOfRetries; i++)
				{
					try
					{
						HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create((this._sourceUrl.Contains("?") ? string.Concat(this._sourceUrl, "&comp=pagelist") : string.Concat(this._sourceUrl, "?comp=pagelist")));
						httpWebRequest.Method = "GET";
						httpWebRequest.ContentLength = (long)0;
						httpWebRequest.Headers["x-ms-version"] = "2011-08-18";
						httpWebRequest.Headers["If-Match"] = this._etag;
						httpWebRequest.AddRange(num, num1);
						using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
						{
							if (response.StatusCode == HttpStatusCode.OK)
							{
								using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
								{
									List<PageRegionsForCopy> pageRegionsForCopies = new List<PageRegionsForCopy>();
									foreach (XElement xElement in XElement.Parse(streamReader.ReadToEnd()).Elements("PageRange"))
									{
										PageRegionsForCopy pageRegionsForCopy = new PageRegionsForCopy()
										{
											Start = long.Parse(xElement.Element("Start").Value),
											End = long.Parse(xElement.Element("End").Value)
										};
										pageRegionsForCopies.Add(pageRegionsForCopy);
									}
									if (pageRegionsForCopies.Count > 0)
									{
										this._pages = pageRegionsForCopies.ToArray();
									}
								}
								break;
							}
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						if (i == this._maxNumOfRetries + 1)
						{
							flag = true;
							throw;
						}
						if (!this.IsRetryableException(exception))
						{
							throw;
						}
					}
					Thread.Sleep(1000);
				}
			}
			catch (Exception exception2)
			{
				this.HandleExceptionAndSetStatus(exception2, flag);
			}
			this._peekedBytes += this._pageRegionFetchRange;
			if (this._pages != null)
			{
				PageRegionsForCopy[] pageRegionsForCopyArray = this._pages;
				for (int j = 0; j < (int)pageRegionsForCopyArray.Length; j++)
				{
					PageRegionsForCopy pageRegionsForCopy1 = pageRegionsForCopyArray[j];
					Logger<IRestProtocolHeadLogger>.Instance.Info.Log("Identified Page:{0}", new object[] { pageRegionsForCopy1 });
				}
			}
		}

		private void HandleExceptionAndSetStatus(Exception ex, bool exhaustedRetries)
		{
			if (exhaustedRetries)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Critical.Log("CopyBlob: HandleExceptionAndSetStatus: Multiple retries failed!");
			}
			if (!(ex is WebException))
			{
				this.SetErrorStatus(HttpStatusCode.InternalServerError, null);
				return;
			}
			HttpWebResponse response = (ex as WebException).Response as HttpWebResponse;
			if (response == null)
			{
				this.SetErrorStatus(HttpStatusCode.InternalServerError, null);
				return;
			}
			this.SetErrorStatus(response.StatusCode, response);
		}

		public bool HasLastOperationFailed()
		{
			return this._httpErrorCode != 200;
		}

		private bool IsRetryableException(Exception e)
		{
			HttpStatusCode statusCode;
			if (e is IOException)
			{
				return true;
			}
			if (e is WebException)
			{
				WebException webException = e as WebException;
				WebExceptionStatus status = webException.Status;
				switch (status)
				{
					case WebExceptionStatus.NameResolutionFailure:
					case WebExceptionStatus.ConnectFailure:
					{
						return true;
					}
					default:
					{
						switch (status)
						{
							case WebExceptionStatus.RequestCanceled:
							case WebExceptionStatus.ConnectionClosed:
							case WebExceptionStatus.KeepAliveFailure:
							case WebExceptionStatus.Timeout:
							case WebExceptionStatus.ProxyNameResolutionFailure:
							case WebExceptionStatus.UnknownError:
							{
								return true;
							}
							case WebExceptionStatus.ProtocolError:
							case WebExceptionStatus.TrustFailure:
							case WebExceptionStatus.SecureChannelFailure:
							case WebExceptionStatus.ServerProtocolViolation:
							case WebExceptionStatus.Pending:
							{
								if (webException.Response == null)
								{
									return false;
								}
								statusCode = ((HttpWebResponse)webException.Response).StatusCode;
								if (statusCode != HttpStatusCode.RequestTimeout)
								{
									switch (statusCode)
									{
										case HttpStatusCode.InternalServerError:
										case HttpStatusCode.BadGateway:
										case HttpStatusCode.ServiceUnavailable:
										case HttpStatusCode.GatewayTimeout:
										{
											break;
										}
										case HttpStatusCode.NotImplemented:
										{
											return false;
										}
										default:
										{
											return false;
										}
									}
								}
								return true;
							}
							default:
							{
								if (webException.Response == null)
								{
									return false;
								}
								statusCode = ((HttpWebResponse)webException.Response).StatusCode;
								if (statusCode != HttpStatusCode.RequestTimeout)
								{
									switch (statusCode)
									{
										case HttpStatusCode.InternalServerError:
										case HttpStatusCode.BadGateway:
										case HttpStatusCode.ServiceUnavailable:
										case HttpStatusCode.GatewayTimeout:
										{
											break;
										}
										case HttpStatusCode.NotImplemented:
										{
											return false;
										}
										default:
										{
											return false;
										}
									}
								}
								return true;
							}
						}
						break;
					}
				}
			}
			return false;
		}

		private void MakeChunks()
		{
			long num = (long)0;
			long num1 = num;
			this._endChunkIndex = num;
			this._currentChunkIndex = num1;
			if ((this._pages[this._currentPageIndex].End - this._pages[this._currentPageIndex].Start + (long)1) % (long)4194304 != (long)0)
			{
				this._endChunkIndex = (this._pages[this._currentPageIndex].End - this._pages[this._currentPageIndex].Start + (long)1) / (long)4194304;
				return;
			}
			this._endChunkIndex = (this._pages[this._currentPageIndex].End - this._pages[this._currentPageIndex].Start + (long)1) / (long)4194304 - (long)1;
		}

		private void PopulateBlocks()
		{
			if (this._isBlockBlob)
			{
				if (!this._isAzureBlob)
				{
					bool flag = this._contentLength % (long)4194304 != (long)0;
					long num = (flag ? this._contentLength / (long)4194304 + (long)1 : this._contentLength / (long)4194304);
					this._blockList = new BlockForCopy[checked((IntPtr)num)];
					for (long i = (long)0; i < num; i += (long)1)
					{
						long num1 = (!flag || i != num - (long)1 ? (long)4194304 : this._contentLength - i * (long)4194304);
						BlockForCopy[] blockForCopyArray = this._blockList;
						IntPtr intPtr = checked((IntPtr)i);
						BlockForCopy blockForCopy = new BlockForCopy();
						Guid guid = Guid.NewGuid();
						blockForCopy.BlockId = DbListBlobObject.ToHexString(guid.ToByteArray());
						blockForCopy.Length = (int)num1;
						blockForCopyArray[intPtr] = blockForCopy;
					}
				}
				else
				{
					this.GetBlockList();
				}
				if (this._blockList != null)
				{
					BlockForCopy[] blockForCopyArray1 = this._blockList;
					for (int j = 0; j < (int)blockForCopyArray1.Length; j++)
					{
						BlockForCopy blockForCopy1 = blockForCopyArray1[j];
						IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
						object[] str = new object[] { blockForCopy1.ToString() };
						info.Log("Identified Blocks:{0}", str);
					}
				}
			}
		}

		private void ReadAllBytes(byte[] dataBuffer, HttpWebResponse response)
		{
			int length = (int)dataBuffer.Length;
			int num = 0;
			Stream responseStream = response.GetResponseStream();
			while (num < length)
			{
				num += responseStream.Read(dataBuffer, num, length - num);
			}
		}

		private void SetErrorStatus(HttpStatusCode status, HttpWebResponse response)
		{
			this._httpErrorCode = (int)status;
			string str = null;
			if (!BlobHttpDownloader.httpErrorCodesMap.TryGetValue(status, out str))
			{
				this._httpDescription = "500 InternalServerError \"Copy failed when reading the source.\" ";
			}
			else
			{
				bool flag = false;
				if (this._isAzureBlob && response != null)
				{
					try
					{
						using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
						{
							XElement xElement = XElement.Parse(streamReader.ReadToEnd());
							CultureInfo invariantCulture = CultureInfo.InvariantCulture;
							object[] value = new object[] { this._httpErrorCode, xElement.Element("Code").Value, "Copy failed when reading the source." };
							this._httpDescription = string.Format(invariantCulture, "{0} {1} \"{2}\"", value);
							flag = true;
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						Logger<IRestProtocolHeadLogger>.Instance.ErrorDebug.Log("CopyBlob: Parse Response for err failed! Ex: {0}", new object[] { exception });
					}
				}
				if (!flag)
				{
					CultureInfo cultureInfo = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { this._httpErrorCode, str, "Copy failed when reading the source." };
					this._httpDescription = string.Format(cultureInfo, "{0} {1} \"{2}\"", objArray);
					return;
				}
			}
		}
	}
}