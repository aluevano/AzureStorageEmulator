using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper5Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper5Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			ProjectedWrapper5 projectedWrapper5 = base.CreateProjectedWrapperInternal<ProjectedWrapper5>(propertyNameList, resourceTypeName);
			projectedWrapper5.ProjectedProperty0 = propertyValues[0];
			projectedWrapper5.ProjectedProperty1 = propertyValues[1];
			projectedWrapper5.ProjectedProperty2 = propertyValues[2];
			projectedWrapper5.ProjectedProperty3 = propertyValues[3];
			projectedWrapper5.ProjectedProperty4 = propertyValues[4];
			return projectedWrapper5;
		}
	}
}