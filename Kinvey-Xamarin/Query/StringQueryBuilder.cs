using System;
using System.Text;

namespace KinveyXamarin
{
	public class StringQueryBuilder : IQueryBuilder
	{
		public StringQueryBuilder(StringBuilder builder)
		{
			this.builder = builder;
		}

		public void Write(object value)
		{
			builder.Append(value);
		}

		private StringBuilder builder;
	}
}