using System;

namespace DD.CBU.Compute.Powershell
{
	using Api.Client;
	using Api.Contracts.Directory;

	/// <summary>
	///		Represents a connection to the CaaS API.
	/// </summary>
	public sealed class ComputeServiceConnection
		: IDisposable
	{
		/// <summary>
		///		The compute API client representing the API client.
		/// </summary>
		ComputeApiClient		_apiClient;

		/// <summary>
		///		Create a new compute service connection.
		/// </summary>
		/// <param name="apiClient">
		///		The CaaS API client represented by the connection.
		/// </param>
		public ComputeServiceConnection(ComputeApiClient apiClient)
		{
			if (apiClient == null)
				throw new ArgumentNullException("apiClient");


			_apiClient = apiClient;
		}

		/// <summary>
		///		The CaaS account targeted by the connection.
		/// </summary>
		public IAccount Account
		{
			get
			{
				return _apiClient.Account;
			}
		}

		/// <summary>
		///		The CaaS API client represented by the connection.
		/// </summary>
		internal ComputeApiClient ApiClient
		{
			get
			{
				return _apiClient;
			}
		}

		/// <summary>
		///		Dispose of resources being used by the CaaS API connection.
		/// </summary>
		public void Dispose()
		{
			if (_apiClient != null)
			{
				_apiClient.Dispose();
				_apiClient = null;
			}
		}
	}
}
