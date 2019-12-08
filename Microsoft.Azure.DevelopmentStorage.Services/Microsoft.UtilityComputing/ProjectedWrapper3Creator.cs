using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper3Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper3Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			ProjectedWrapper3 projectedWrapper3 = base.CreateProjectedWrapperInternal<ProjectedWrapper3>(propertyNameList, resourceTypeName);
			projectedWrapper3.ProjectedProperty0 = propertyValues[0];
			projectedWrapper3.ProjectedProperty1 = propertyValues[1];
			projectedWrapper3.ProjectedProperty2 = propertyValues[2];
			return projectedWrapper3;
		}
	}
}