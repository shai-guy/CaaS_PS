using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace DD.CBU.Compute.Api.Client
{	
	using Contracts.Datacenter;
	using Contracts.Directory;
	using Contracts.Server;
	using Utilities;

	/// <summary>
	///		A client for the Dimension Data Compute-as-a-Service (CaaS) API.
	/// </summary>
    public sealed class ComputeApiClient
		: DisposableObject
	{
		#region Instance data

		/// <summary>
		///		Media type formatters used to serialise and deserialise data contracts when communicating with the CaaS API.
		/// </summary>
		readonly MediaTypeFormatterCollection	_mediaTypeFormatters = new MediaTypeFormatterCollection();

		/// <summary>
		///		The <see cref="HttpMessageHandler"/> used to customise communications with the CaaS API.
		/// </summary>
		HttpClientHandler						_clientMessageHandler = new HttpClientHandler();

		/// <summary>
		///		The <see cref="HttpClient"/> used to communicate with the CaaS API.
		/// </summary>
		HttpClient								_httpClient;

		/// <summary>
		///		The details for the CaaS account associated with the supplied credentials.
		/// </summary>
		Account									_account;

		#endregion // Instance data
		
		#region Construction / disposal

		/// <summary>
		///		Create a new Compute-as-a-Service API client.
		/// </summary>
		/// <param name="targetRegionName">
		///		The name of the region whose CaaS API end-point is targeted by the client.
		/// </param>
		public ComputeApiClient(string targetRegionName)
		{
			if (String.IsNullOrWhiteSpace(targetRegionName))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'targetRegionName'.", "targetRegionName");

			_mediaTypeFormatters.XmlFormatter.UseXmlSerializer = true;
			_httpClient = new HttpClient(_clientMessageHandler);
			_httpClient.BaseAddress = ApiUris.ComputeBase(targetRegionName);
		}

		/// <summary>
		///		Dispose of resources being used by the CaaS API client.
		/// </summary>
		/// <param name="disposing">
		///		Explicit disposal?
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_clientMessageHandler != null)
				{
					_clientMessageHandler.Dispose();
					_clientMessageHandler = null;
				}

				if (_httpClient != null)
				{
					_httpClient.Dispose();
					_httpClient = null;
				}

				_account = null;
			}
		}

		#endregion // Construction / disposal

		#region Public properties

		/// <summary>
		///		Read-only information about the CaaS account targeted by the CaaS API client.
		/// </summary>
		/// <remarks>
		///		<c>null</c>, unless logged in.
		/// </remarks>
		/// <seealso cref="LoginAsync"/>
		public IAccount Account
		{
			get
			{
				CheckDisposed();

				return _account;
			}
		}

		/// <summary>
		///		Is the API client currently logged in to the CaaS API?
		/// </summary>
		public bool IsLoggedIn
		{
			get
			{
				CheckDisposed();

				return _account != null;
			}
		}

		#endregion // Public properties

		#region Public methods

		/// <summary>
		///		Asynchronously log into the CaaS API.
		/// </summary>
		/// <param name="accountCredentials">
		///		The CaaS account credentials used to authenticate against the CaaS API.
		/// </param>
		/// <returns>
		///		An <see cref="IAccount"/> implementation representing the CaaS account that the client is logged into.
		/// </returns>
		public async Task<IAccount> LoginAsync(ICredentials accountCredentials)
		{
			if (accountCredentials == null)
				throw new ArgumentNullException("accountCredentials");

			CheckDisposed();

			if (_account != null)
				throw ComputeApiClientException.AlreadyLoggedIn();

			_clientMessageHandler.Credentials = accountCredentials;
			_clientMessageHandler.PreAuthenticate = true;

			try
			{
				_account = await ApiGetAsync<Account>(ApiUris.MyAccount);
			}
			catch (HttpRequestException eRequestFailure)
			{
				Debug.WriteLine(eRequestFailure.GetBaseException(), "BASE EXCEPTION");

				throw;
			}
			Debug.Assert(_account != null, "_account != null");

			return _account;
		}

		/// <summary>
		///		Log out of the CaaS API.
		/// </summary>
		public void Logout()
		{
			CheckDisposed();

			if (_account == null)
				throw ComputeApiClientException.NotLoggedIn();

			_account = null;
			_clientMessageHandler.Credentials = null;
			_clientMessageHandler.PreAuthenticate = false;
		}

		/// <summary>
		///		Asynchronously get a list of all CaaS data centres that are available for use by the specified organisation.
		/// </summary>
		/// <param name="organizationId">
		///		The organisation Id.
		/// </param>
		/// <returns>
		///		A read-only list of <see cref="IDatacenterDetail"/>s representing the data centre information.
		/// </returns>
		public async Task<IReadOnlyList<IDatacenterDetail>> GetAvailableDataCenters(Guid organizationId)
		{
			CheckDisposed();

			DatacentersWithDiskSpeedDetails datacentersWithDiskSpeedDetails =
				await ApiGetAsync<DatacentersWithDiskSpeedDetails>(
					ApiUris.DatacentersWithDiskSpeedDetails(
						organizationId
					)
				);

			return datacentersWithDiskSpeedDetails.Datacenters;
		}

		/// <summary>
		///		Get a list of all system-defined images (with software labels) deployed in the specified data centre.
		/// </summary>
		/// <param name="locationName">
		///		The short name of the location in which the data centre is located.
		/// </param>
		/// <returns>
		///		A read-only list <see cref="ImageDetail"/>, sorted by UTC creation date / time, representing the images.
		/// </returns>
		public async Task<IReadOnlyList<IImageDetail>> GetImages(string locationName)
		{
			if (String.IsNullOrWhiteSpace(locationName))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'locationName'.", "locationName");

			ImagesWithSoftwareLabels imagesWithSoftwareLabels =
				await ApiGetAsync<ImagesWithSoftwareLabels>(
					ApiUris.ImagesWithSoftwareLabels(locationName)
				);

			return imagesWithSoftwareLabels.Images;
		}

		#endregion // Public methods

		#region WebAPI invocation

		/// <summary>
		///		Invoke a CaaS API operation using a HTTP GET request.
		/// </summary>
		/// <typeparam name="TResult">
		///		The XML-serialisable data contract type into which the response will be deserialised.
		/// </typeparam>
		/// <param name="relativeOperationUri">
		///		The operation URI (relative to the CaaS API's base URI).
		/// </param>
		/// <returns>
		///		The operation result.
		/// </returns>
		async Task<TResult> ApiGetAsync<TResult>(Uri relativeOperationUri)
		{
			if (relativeOperationUri == null)
				throw new ArgumentNullException("relativeOperationUri");

			if (relativeOperationUri.IsAbsoluteUri)
				throw new ArgumentException("The supplied URI is not a relative URI.", "relativeOperationUri");

			CheckDisposed();

			using (HttpResponseMessage response = await _httpClient.GetAsync(relativeOperationUri))
			{
				if (response.IsSuccessStatusCode)
					return await response.Content.ReadAsAsync<TResult>(_mediaTypeFormatters);

				switch (response.StatusCode)
				{
					case HttpStatusCode.Unauthorized:
					{
						throw ComputeApiException.InvalidCredentials(
							(
								(NetworkCredential)_clientMessageHandler.Credentials
							)
							.UserName
						);
					}
					default:
					{
						throw new HttpRequestException(
							String.Format(
								"CaaS API returned HTTP status code {0} ({1}) when performing HTTP GET on '{2}'.",
								(int)response.StatusCode,
								response.StatusCode,
								new Uri(
									_httpClient.BaseAddress,
									relativeOperationUri
								)
							)
						);
					}
				}
			}
		}

		#endregion // WebAPI invocation
	}
}
