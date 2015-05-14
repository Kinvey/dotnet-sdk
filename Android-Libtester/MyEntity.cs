using System;
using RestSharp;


namespace AndroidLibtester
{
	public class MyEntity
	{
		public MyEntity(string id){
			this.ID = id;
		}

		public MyEntity(){}

		public string ID {get; set;}


		public string Name{get;set;}


		public string Email{get;set;}


		public string lowercasetest{get;set;}


		public bool IsAvailable{get; set;}

	}
}

