using Microsoft.UtilityComputing;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbUtilityResourceBuilder
	{
		public DbUtilityResourceBuilder()
		{
		}

		public static object GetProjectedWrapperEnumerator(IEnumerator<TableRow> enumerator, int propertyCount, string propertyListName, string resourceTypeName, string[] projectedProperties)
		{
			switch (Math.Min(9, propertyCount))
			{
				case 0:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper0>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 1:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper1>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 2:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper2>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 3:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper3>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 4:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper4>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 5:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper5>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 6:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper6>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 7:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper7>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 8:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapper8>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
				case 9:
				{
					return DbUtilityResourceBuilder.GetProjectedWrapperTypedEnumerator<ProjectedWrapperMany>(enumerator, propertyCount, propertyListName, resourceTypeName, projectedProperties);
				}
			}
			return null;
		}

		public static IEnumerator<T> GetProjectedWrapperTypedEnumerator<T>(IEnumerator<TableRow> enumerator, int propertyCount, string propertyListName, string resourceTypeName, string[] projectedProperties)
		where T : ProjectedWrapper
		{
			List<T> ts = new List<T>();
			while (enumerator.MoveNext())
			{
				TableRow current = enumerator.Current;
				Dictionary<string, object> strs = new Dictionary<string, object>();
				strs["PartitionKey"] = DevelopmentStorageDbDataContext.DecodeKeyString(current.PartitionKey);
				strs["RowKey"] = DevelopmentStorageDbDataContext.DecodeKeyString(current.RowKey);
				strs["Timestamp"] = current.Timestamp;
				XmlUtility.AddObjectsToDictionaryFromXml(current.Data, strs, true);
				object[] objArray = new object[propertyCount];
				for (int i = 0; i < propertyCount; i++)
				{
					if (!strs.TryGetValue(projectedProperties[i], out objArray[i]))
					{
						objArray[i] = null;
					}
				}
				ProjectedWrapperCreator projectedWrapperCreator = ProjectedWrapperCreator.GetProjectedWrapperCreator(propertyCount);
				ts.Add((T)projectedWrapperCreator.CreateProjectedWrapper(objArray, propertyListName, resourceTypeName));
			}
			return ts.GetEnumerator();
		}

		public static object GetUtilityRowEnumerator(IEnumerator<TableRow> enumerator)
		{
			List<UtilityRow> utilityRows = new List<UtilityRow>();
			while (enumerator.MoveNext())
			{
				TableRow current = enumerator.Current;
				UtilityRow utilityRow = new UtilityRow()
				{
					PartitionKey = DevelopmentStorageDbDataContext.DecodeKeyString(current.PartitionKey),
					RowKey = DevelopmentStorageDbDataContext.DecodeKeyString(current.RowKey),
					Timestamp = current.Timestamp
				};
				XmlUtility.AddObjectsToDictionaryFromXml(current.Data, utilityRow, false);
				utilityRows.Add(utilityRow);
			}
			return utilityRows.GetEnumerator();
		}

		public static object GetUtilityTableEnumerator(IEnumerator<TableContainer> enumerator)
		{
			List<UtilityTable> utilityTables = new List<UtilityTable>();
			while (enumerator.MoveNext())
			{
				TableContainer current = enumerator.Current;
				utilityTables.Add(new UtilityTable()
				{
					TableName = current.CasePreservedTableName
				});
			}
			return utilityTables.GetEnumerator();
		}
	}
}