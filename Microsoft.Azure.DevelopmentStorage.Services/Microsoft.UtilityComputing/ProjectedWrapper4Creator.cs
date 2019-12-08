using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper4Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper4Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			ProjectedWrapper4 projectedWrapper4 = base.CreateProjectedWrapperInternal<ProjectedWrapper4>(propertyNameList, resourceTypeName);
			projectedWrapper4.ProjectedProperty0 = propertyValues[0];
			projectedWrapper4.ProjectedProperty1 = propertyValues[1];
			projectedWrapper4.ProjectedProperty2 = propertyValues[2];
			projectedWrapper4.ProjectedProperty3 = propertyValues[3];
			return projectedWrapper4;
		}
	}
}