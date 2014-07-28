using System;

namespace KinveyXamarin
{
	public interface IQueryBuilder
	{
		void Write(object value);

		string GetString();
	}
}

