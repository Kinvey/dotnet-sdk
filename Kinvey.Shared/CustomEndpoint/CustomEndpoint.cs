// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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

namespace Kinvey
{
    /// <summary>
    /// Custom endpoint.
    /// </summary>
    /// <typeparam name="I">The type of request.</typeparam>
    /// <typeparam name="O">The type of response.</typeparam>
    public class CustomEndpoint<I, O>
	{
		private AbstractClient client;

		private JObject customRequestProperties = new JObject();

		/// <summary>
		/// Sets the custom request properties.
		/// </summary>
		/// <param name="customheaders">Custom headers.</param>
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
        /// Initializes a new instance of the <see cref="CustomEndpoint{I,O}"/> class.
        /// </summary>
        /// <param name="client">Client that the user is logged in.</param>
        public CustomEndpoint(AbstractClient client)
		{
			this.client = client;
			customRequestProperties = client.GetCustomRequestProperties();
		}

        /// <summary>
        /// Executes the custom endpoint, expecting a single result.
        /// </summary>
        /// <param name="endpoint">Endpoint name.</param>
        /// <param name="input">Input object.</param>
        /// <param name="client">[optional] Client that the user is logged in for, defaulted to SharedClient.</param>
        /// <returns>The asynchronous task with response result.</returns>  
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
				result = await BuildCustomEnpointRequest(endpoint, input).ExecuteAsync().ConfigureAwait(false);
			}
			catch (KinveyException ke)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_CUSTOM_ENDPOINT, EnumErrorCode.ERROR_CUSTOM_ENDPOINT_ERROR, "", e);
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

			custom.customRequestHeaders = GetCustomRequestProperties();

			return custom;
		}
	}
}
