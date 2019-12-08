using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class TableContainer : Container, ITableContainer, IContainer, IDisposable
	{
		public override Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType ContainerType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.TableContainer;
			}
		}

		protected new ITableContainer InternalContainer
		{
			get
			{
				return (ITableContainer)base.InternalContainer;
			}
		}

		internal TableContainer(ITableContainer tableContainer) : base(tableContainer)
		{
		}
	}
}