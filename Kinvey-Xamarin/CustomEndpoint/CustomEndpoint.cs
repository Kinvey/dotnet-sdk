// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	/// <summary>
	/// Custom endpoint.
	/// </summary>
	public class CustomEndpoint<I, O>
	{

		private AbstractClient client;

//		private string clientAppVersion = null;

		private JObject customRequestProperties = new JObject();

//		public void SetClientAppVersion(string appVersion){
//			this.clientAppVersion = appVersion;	
//		}
//
//		public void SetClientAppVersion(int major, int minor, int revision){
//			SetClientAppVersion(major + "." + minor + "." + revision);
//		}
//
//		public string GetClientAppVersion(){
//			return this.clientAppVersion;
//		}

		/// <summary>
		/// Sets the custom request properties.
		/// </summary>
		/// <param name="customheaders">Customheaders.</param>
		public void SetCustomRequestProperties(JObject customheaders)
		{
			customRequestProperties = customheaders;
		}

		/// <summary>
		/// Sets the custom request property.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void SetCustomRequestProperty(string key, JObject value)
		{
			if (customRequestProperties == null)
			{
				customRequestProperties = new JObject();
			}

			customRequestProperties.Add (key, value);
		}

		/// <summary>
		/// Clears the custom request properties.
		/// </summary>
		public void ClearCustomRequestProperties()
		{
			customRequestProperties = new JObject();
		}

		/// <summary>
		/// Gets the custom request properties.
		/// </summary>
		/// <returns>The custom request properties.</returns>
		public JObject GetCustomRequestProperties()
		{
			return customRequestProperties;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:KinveyXamarin.CustomEndpoint`2"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		public CustomEndpoint(AbstractClient client)
		{
			this.client = client;
			customRequestProperties = client.GetCustomRequestProperties();
			//this.clientAppVersion = client.GetClientAppVersion ();
		}

		/// <summary>
		/// Executes the custom endpoint, expecting a single result
		/// </summary>
		/// <param name="endpoint">Endpoint name.</param>
		/// <param name="input">Input object.</param>
		/// <param name="client">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		public async Task<O> ExecuteCustomEndpoint(string endpoint, I input, AbstractClient client = null)
		{
			this.client = client;
			if (this.client == null)
			{
				this.client = Client.SharedClient;
			}

			O result = default(O);
			try
			{
				result = await BuildCustomEnpointRequest(endpoint, input).ExecuteAsync();
			}
			catch (Exception e)
			{
				//throw new KinveyException(EnumErrorCategory, EnumErrorCode, "", e);
			}

			return result;
		}

		/// <summary>
		/// Executes the custom endpoint, expecting a single result
		/// </summary>
		/// <param name="endpoint">Endpoint name.</param>
		/// <param name="input">Input object.</param>
		/// <param name="client">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
		public async Task<O[]> ExecuteCustomEndpointArray(string endpoint, I input, AbstractClient client = null)
		{
			this.client = client;
			if (this.client == null)
			{
				this.client = Client.SharedClient;
			}

			O[] result = default(O[]);
			try
			{
				result = await BuildCustomEnpointArrayRequest(endpoint, input).ExecuteAsync();
			}
			catch (Exception e)
			{
				//throw new KinveyException(EnumErrorCategory, EnumErrorCode, "", e);
			}

			return result;
		}

		/// <summary>
		/// Executes the custom endpoint blocking.
		/// </summary>
		/// <returns>The custom endpoint blocking.</returns>
		/// <param name="endpoint">Endpoint.</param>
		/// <param name="input">Input.</param>
		internal CustomEndpointRequest<I, O> BuildCustomEnpointRequest(string endpoint, I input)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("endpoint", endpoint);

			CustomEndpointRequest<I, O> custom = new CustomEndpointRequest<I, O>(client, endpoint, input, urlParameters);

			client.InitializeRequest(custom);
			//custom.clientAppVersion = this.GetClientAppVersion ();
			custom.customRequestHeaders = GetCustomRequestProperties();
			return custom;
		}

		/// <summary>
		/// Executes the custom endpoint array blocking.
		/// </summary>
		/// <returns>The custom endpoint array blocking.</returns>
		/// <param name="endpoint">Endpoint.</param>
		/// <param name="input">Input.</param>
		internal CustomEndpointArrayRequest<I, O> BuildCustomEnpointArrayRequest(string endpoint, I input)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("endpoint", endpoint);

			CustomEndpointArrayRequest<I, O> custom = new CustomEndpointArrayRequest<I, O>(client, endpoint, input, urlParameters);

			client.InitializeRequest(custom);
			//custom.clientAppVersion = this.GetClientAppVersion ();
			custom.customRequestHeaders = GetCustomRequestProperties();
			return custom;
		}
	}
}
