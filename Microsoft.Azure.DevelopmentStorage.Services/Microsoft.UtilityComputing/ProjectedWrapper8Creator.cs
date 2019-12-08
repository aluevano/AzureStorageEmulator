using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper8Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper8Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			ProjectedWrapper8 projectedWrapper8 = base.CreateProjectedWrapperInternal<ProjectedWrapper8>(propertyNameList, resourceTypeName);
			projectedWrapper8.ProjectedProperty0 = propertyValues[0];
			projectedWrapper8.ProjectedProperty1 = propertyValues[1];
			projectedWrapper8.ProjectedProperty2 = propertyValues[2];
			projectedWrapper8.ProjectedProperty3 = propertyValues[3];
			projectedWrapper8.ProjectedProperty4 = propertyValues[4];
			projectedWrapper8.ProjectedProperty5 = propertyValues[5];
			projectedWrapper8.ProjectedProperty6 = propertyValues[6];
			projectedWrapper8.ProjectedProperty7 = propertyValues[7];
			return projectedWrapper8;
		}
	}
}