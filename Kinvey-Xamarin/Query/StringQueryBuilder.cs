using System;
using System.Text;

namespace KinveyXamarin
{
	public class StringQueryBuilder : IQueryBuilder
	{

		private StringBuilder builder;
		private StringBuilder dangler;

		public StringQueryBuilder()
		{
			Reset ();
		}

		public void Reset(){
			this.builder = new StringBuilder ();
			this.dangler = new StringBuilder ();
		}

		public void Write(object value)
		{
			builder.Append(value);
		}

		public String GetFullString(){
			return builder.ToString () + dangler.ToString ();
		}

		public void Dangle(object value){
			dangler.Append (value);

		}
	}

}