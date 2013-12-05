using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DD.CBU.Compute.Api.Client
{
	using Contracts.Directory;

	/// <summary>
	///		A client for the Dimension Data Compute-as-a-Service (CaaS) API.
	/// </summary>
    public sealed class ComputeApiClient
		: IDisposable
	{
		/// <summary>
		///		The <see cref="HttpMessageHandler"/> used to customise communications with the CaaS API.
		/// </summary>
		HttpClientHandler	_clientMessageHandler;

		/// <summary>
		///		The <see cref="HttpClient"/> used to communicate with the CaaS API.
		/// </summary>
		HttpClient			_httpClient;

		/// <summary>
		///		The details for the CaaS account associated with the supplied credentials.
		/// </summary>
		Account				_account;

		/// <summary>
		///		Has the client been disposed?
		/// </summary>
		bool				_isDisposed;

		#region Construction / disposal

		/// <summary>
		///		Create a new Compute-as-a-Service API client.
		/// </summary>
		/// <param name="regionName">
		///		The name of the region whose CaaS API is targeted by the client.
		/// </param>
		public ComputeApiClient(string regionName)
		{
			if (String.IsNullOrWhiteSpace(regionName))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'regionName'.", "regionName");

			_httpClient = new HttpClient(_clientMessageHandler);
			_httpClient.BaseAddress = new Uri(
				String.Format(
					"https://api-{0}.dimensiondata.com/oec/0.9/myaccount",
					regionName
				)
			);
		}

		/// <summary>
		///		Dispose of resources being used by the CaaS client.
		/// </summary>
		public void Dispose()
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

			_isDisposed = true; // We don't use a guard for this because a previous disposal may have completed with only partial success.
		}

		/// <summary>
		///		Check if the client has been disposed.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		///		The client has been disposed.
		/// </exception>
		void CheckDisposed()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		#endregion // Construction / disposal
		
		/// <summary>
		///		Is the API client currently logged in to the CaaS API?
		/// </summary>
		public bool LoggedIn
		{
			get
			{
				CheckDisposed();

				return _account != null;
			}
		}

		/// <summary>
		///		Asynchronously log into the CaaS API.
		/// </summary>
		/// <param name="accountCredentials">
		///		The CaaS account credentials used to authenticate against the CaaS API.
		/// </param>
		/// <returns>
		///		A <see cref="Task"/> representing the asynchronous login operation.
		/// </returns>
		public async Task LoginAsync(ICredentials accountCredentials)
		{
			if (accountCredentials == null)
				throw new ArgumentNullException("accountCredentials");

			CheckDisposed();

			if (_account != null)
				throw new InvalidOperationException("Already logged in.");

			_clientMessageHandler.Credentials = accountCredentials;
			_clientMessageHandler.PreAuthenticate = true;

			HttpResponseMessage response = await _httpClient.GetAsync("account");
			response.EnsureSuccessStatusCode(); // TODO: Better error-handling.

			_account = await response.Content.ReadAsAsync<Account>();
		}

		/// <summary>
		///		Log out of the CaaS API.
		/// </summary>
		public void Logout()
		{
			CheckDisposed();

			if (_account == null)
				throw new InvalidOperationException("Not currently logged in.");

			_account = null;
			_clientMessageHandler.Credentials = null;
			_clientMessageHandler.PreAuthenticate = false;
		}
    }
}
