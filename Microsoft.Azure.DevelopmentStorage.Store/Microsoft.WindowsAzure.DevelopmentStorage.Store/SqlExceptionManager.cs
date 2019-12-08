using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest;
using System;
using System.Collections;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class SqlExceptionManager
	{
		private static Regex s_duplicateKeyRegex;

		static SqlExceptionManager()
		{
			SqlExceptionManager.s_duplicateKeyRegex = new Regex("Cannot insert duplicate key in object '([^']+)'", RegexOptions.Compiled);
		}

		public SqlExceptionManager()
		{
		}

		internal static void ReThrowException(Exception e)
		{
			throw SqlExceptionManager.TransformSqlException(e);
		}

		internal static Exception TransformSqlException(Exception e)
		{
			Exception containerNotFoundException;
			if (e is ArgumentOutOfRangeException)
			{
				return new XStoreArgumentOutOfRangeException(e.Message, e);
			}
			if (e is ArgumentException)
			{
				return new XStoreArgumentException(e.Message, e);
			}
			if (e is InvalidOperationException)
			{
				if (e.Message.Contains("Null values are not supported in key members"))
				{
					return new TableServiceGeneralException(TableServiceError.PropertiesNeedValue, e);
				}
				if (!e.Message.Contains("Resource not found for the segment"))
				{
					goto Label1;
				}
				string message = e.Message;
				char[] chrArray = new char[] { '\'' };
				string str = message.Split(chrArray)[1];
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					if ((
						from t in dbContext.TableContainers
						where t.TableName == str
						select t).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer>() != null)
					{
						containerNotFoundException = e;
						return containerNotFoundException;
					}
					else
					{
						containerNotFoundException = e;
						return containerNotFoundException;
					}
				}
			}
		Label1:
			if (!(e is SqlException))
			{
				return e;
			}
			IEnumerator enumerator = ((SqlException)e).Errors.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SqlError current = (SqlError)enumerator.Current;
					if (current.Number == 547)
					{
						if (!current.Message.Contains("TableContainer_TableRow"))
						{
							continue;
						}
						containerNotFoundException = new ContainerNotFoundException();
						return containerNotFoundException;
					}
					else if (current.Number != 2627)
					{
						if (current.Number != 8152)
						{
							if (current.Number != 50000)
							{
								continue;
							}
							if (current.Message == "BlockIdMismatch")
							{
								containerNotFoundException = new InvalidBlockException();
								return containerNotFoundException;
							}
							else if (current.Message == "InvalidBlockList")
							{
								containerNotFoundException = new InvalidBlockListException();
								return containerNotFoundException;
							}
							else if (current.Message == "BlobHasSnapshots")
							{
								containerNotFoundException = new SnapshotsPresentException();
								return containerNotFoundException;
							}
							else if (current.Message != "BlobHasNoSnapshots")
							{
								if (current.Message != "EntityTooLarge")
								{
									continue;
								}
								containerNotFoundException = new TableServiceGeneralException(TableServiceError.EntityTooLarge, e);
								return containerNotFoundException;
							}
							else
							{
								containerNotFoundException = new BlobNotFoundException();
								return containerNotFoundException;
							}
						}
						else
						{
							containerNotFoundException = new TableServiceGeneralException(TableServiceError.PropertyValueTooLarge, e);
							return containerNotFoundException;
						}
					}
					else if (current.Message.Contains(string.Concat("'dbo.", typeof(Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer).Name, "'")))
					{
						containerNotFoundException = new ContainerAlreadyExistsException();
						return containerNotFoundException;
					}
					else if (current.Message.Contains(string.Concat("'dbo.", typeof(Blob).Name, "'")))
					{
						containerNotFoundException = new BlobAlreadyExistsException();
						return containerNotFoundException;
					}
					else if (current.Message.Contains(string.Concat("'dbo.", typeof(Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer).Name, "'")))
					{
						containerNotFoundException = new ContainerAlreadyExistsException();
						return containerNotFoundException;
					}
					else if (!current.Message.Contains(string.Concat("'dbo.", typeof(Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer).Name, "'")))
					{
						if (!current.Message.Contains(string.Concat("'dbo.", typeof(TableRow).Name, "'")))
						{
							continue;
						}
						containerNotFoundException = new TableServiceGeneralException(TableServiceError.EntityAlreadyExists, e);
						return containerNotFoundException;
					}
					else
					{
						containerNotFoundException = new ContainerAlreadyExistsException();
						return containerNotFoundException;
					}
				}
				return e;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return containerNotFoundException;
		}
	}
}