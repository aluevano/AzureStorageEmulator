using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapperManyCreator : ProjectedWrapperCreator
	{
		public ProjectedWrapperManyCreator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			object[] objArray;
			ProjectedWrapperMany projectedWrapperMany = base.CreateProjectedWrapperInternal<ProjectedWrapperMany>(propertyNameList, resourceTypeName);
			if ((int)propertyValues.Length % 8 == 0)
			{
				objArray = propertyValues;
			}
			else
			{
				objArray = new object[((int)propertyValues.Length / 8 + 1) * 8];
				propertyValues.CopyTo(objArray, 0);
			}
			projectedWrapperMany.ProjectedProperty0 = objArray[0];
			projectedWrapperMany.ProjectedProperty1 = objArray[1];
			projectedWrapperMany.ProjectedProperty2 = objArray[2];
			projectedWrapperMany.ProjectedProperty3 = objArray[3];
			projectedWrapperMany.ProjectedProperty4 = objArray[4];
			projectedWrapperMany.ProjectedProperty5 = objArray[5];
			projectedWrapperMany.ProjectedProperty6 = objArray[6];
			projectedWrapperMany.ProjectedProperty7 = objArray[7];
			ProjectedWrapperMany next = projectedWrapperMany;
			for (int i = 1; i < (int)objArray.Length / 8; i++)
			{
				next.Next = this.CreateProjectedWrapperMany(objArray, i * 8);
				next = next.Next;
			}
			next.Next = new ProjectedWrapperManyEnd();
			return projectedWrapperMany;
		}

		private ProjectedWrapperMany CreateProjectedWrapperMany(object[] propertyValues, int startIndex)
		{
			ProjectedWrapperMany projectedWrapperMany = new ProjectedWrapperMany()
			{
				ProjectedProperty0 = propertyValues[startIndex],
				ProjectedProperty1 = propertyValues[startIndex + 1],
				ProjectedProperty2 = propertyValues[startIndex + 2],
				ProjectedProperty3 = propertyValues[startIndex + 3],
				ProjectedProperty4 = propertyValues[startIndex + 4],
				ProjectedProperty5 = propertyValues[startIndex + 5],
				ProjectedProperty6 = propertyValues[startIndex + 6],
				ProjectedProperty7 = propertyValues[startIndex + 7]
			};
			return projectedWrapperMany;
		}
	}
}