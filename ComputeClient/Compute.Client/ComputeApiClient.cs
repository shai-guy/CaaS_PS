using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DD.CBU.Compute.Api.Client
{
	using System.Net.Http.Formatting;
	using Contracts.Directory;

	/// <summary>
	///		A client for the Dimension Data Compute-as-a-Service (CaaS) API.
	/// </summary>
	/// <remarks>
	///		This client is async but, from a concurrency perspective, it not thread-safe.
	/// </remarks>
    public sealed class ComputeApiClient
		: IDisposable
	{
		/// <summary>
		///		The <see cref="HttpMessageHandler"/> used to customise communications with the CaaS API.
		/// </summary>
		HttpClientHandler	_clientMessageHandler = new HttpClientHandler();

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
					"https://api-{0}.dimensiondata.com/oec/0.9/",
					regionName
				)
			);
		}

		/// <summary>
		///		Dispose of resources being used by the CaaS client.
		/// </summary>
		public void Dispose()
		{
			if (_isDisposed)
				return;

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

			_isDisposed = true;
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
		///		An <see cref="IAccount"/> implementation representing the CaaS account that the client is logged into.
		/// </returns>
		public async Task<IAccount> LoginAsync(ICredentials accountCredentials)
		{
			if (accountCredentials == null)
				throw new ArgumentNullException("accountCredentials");

			CheckDisposed();

			if (_account != null)
				throw new InvalidOperationException("Already logged in.");

			_clientMessageHandler.Credentials = accountCredentials;
			_clientMessageHandler.PreAuthenticate = true;

			HttpResponseMessage response = await _httpClient.GetAsync(ApiUrls.MyAccount);
			response.EnsureSuccessStatusCode(); // TODO: Better error-handling.

			_account = await response.Content.XmlDeserializeAsync<Account>();

			return _account;
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
