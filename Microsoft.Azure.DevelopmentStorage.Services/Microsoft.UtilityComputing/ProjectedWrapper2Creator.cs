using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper2Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper2Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			ProjectedWrapper2 projectedWrapper2 = base.CreateProjectedWrapperInternal<ProjectedWrapper2>(propertyNameList, resourceTypeName);
			projectedWrapper2.ProjectedProperty0 = propertyValues[0];
			projectedWrapper2.ProjectedProperty1 = propertyValues[1];
			return projectedWrapper2;
		}
	}
}