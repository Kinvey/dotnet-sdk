using System;

namespace KinveyXamarin
{
	public interface IQueryBuilder
	{
		void Write(object value);

		string GetFullString();

		void Dangle(object value);

		void Reset();
	}
}

