using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public abstract class ProjectedWrapperCreator
	{
		private static ProjectedWrapperCreator[] wrapperCreators;

		static ProjectedWrapperCreator()
		{
			ProjectedWrapperCreator[] projectedWrapper0Creator = new ProjectedWrapperCreator[] { new ProjectedWrapper0Creator(), new ProjectedWrapper1Creator(), new ProjectedWrapper2Creator(), new ProjectedWrapper3Creator(), new ProjectedWrapper4Creator(), new ProjectedWrapper5Creator(), new ProjectedWrapper6Creator(), new ProjectedWrapper7Creator(), new ProjectedWrapper8Creator(), new ProjectedWrapperManyCreator() };
			ProjectedWrapperCreator.wrapperCreators = projectedWrapper0Creator;
		}

		protected ProjectedWrapperCreator()
		{
		}

		public abstract ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName);

		protected T CreateProjectedWrapperInternal<T>(string propertyNameList, string resourceTypeName)
		where T : ProjectedWrapper, new()
		{
			T t = Activator.CreateInstance<T>();
			t.PropertyNameList = propertyNameList;
			t.ResourceTypeName = resourceTypeName;
			return t;
		}

		public static ProjectedWrapperCreator GetProjectedWrapperCreator(int propertyCount)
		{
			propertyCount = Math.Min(9, propertyCount);
			return ProjectedWrapperCreator.wrapperCreators[propertyCount];
		}
	}
}