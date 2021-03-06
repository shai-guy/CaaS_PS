﻿using System.Linq;

using DD.CBU.Compute.Api.Contracts.Software;

namespace DD.CBU.Compute.Api.Client
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Threading.Tasks;

	using DD.CBU.Compute.Api.Client.Interfaces;
	using DD.CBU.Compute.Api.Client.Utilities;
	using DD.CBU.Compute.Api.Contracts.Datacenter;
	using DD.CBU.Compute.Api.Contracts.Directory;
	using DD.CBU.Compute.Api.Contracts.General;
	using DD.CBU.Compute.Api.Contracts.Server;

	/// <summary>
    ///		A client for the Dimension Data Compute-as-a-Service (CaaS) API.
    /// </summary>
    public sealed class ComputeApiClient
        : DisposableObject, IComputeApiClient
    {  
        #region Instance data

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

            WebApi = new WebApi(targetRegionName);
        }

        /// <summary>
        /// Creates a new CaaS API client using a base URI.
        /// </summary>
        /// <param name="baseUri">The base URI to use for the CaaS API.</param>
        public ComputeApiClient(Uri baseUri)
        {
            if (baseUri == null)
                throw new ArgumentNullException("baseUri", "Argument cannot be null");

            if (!baseUri.IsAbsoluteUri)
                throw new ArgumentException("Base URI supplied is not an absolute URI", "baseUri");

            WebApi = new WebApi(baseUri);
        }

        /// <summary>
        /// Creates a new CaaS API client using a base URI.
        /// </summary>
        public ComputeApiClient(IHttpClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client", "Argument cannot be null");

            WebApi = new WebApi(client);
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
                if (WebApi != null)
                {
                    WebApi.Dispose();
                    WebApi = null;
                }
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
        public IAccount Account
        {
            get
            {
				CheckDisposed();

                return WebApi.Account;
            }
        }

        /// <summary>
        /// Access to the web API for login/logout and account info
        /// </summary>
        public IWebApi WebApi { get; private set; }

        /// <summary>
        ///	Asynchronously log into the CaaS API.
        /// </summary>
        /// <param name="accountCredentials">
        ///	The CaaS account credentials used to authenticate against the CaaS API.
        /// </param>
        /// <returns>
        ///	An <see cref="IAccount"/> implementation representing the CaaS account that the client is logged into.
        /// </returns>
        public async Task<IAccount> LoginAsync(ICredentials accountCredentials)
        {
	        if (accountCredentials == null)
		        throw new ArgumentNullException("accountCredentials", "Account credentials cannot be null!");

			CheckDisposed();

			return await WebApi.LoginAsync(accountCredentials);
        }

        /// <summary>
        ///	Log out of the CaaS API.
        /// </summary>
        public void Logout()
        {
			CheckDisposed();
			
			WebApi.Logout();
        }

        /// <summary>
        /// Gets a list of software labels
        /// </summary>
        /// <returns>A list of software labels</returns>
        public async Task<IReadOnlyCollection<SoftwareLabel>> GetListOfSoftwareLabelsAsync()
        {
			CheckDisposed();
			
			var relativeUrl = string.Format("{0}/softwarelabel", Account.OrganizationId);
            var uri = new Uri(relativeUrl, UriKind.Relative);

            var labels = await WebApi.ApiGetAsync<SoftwareLabels>(uri);

            return (labels.Items);
        }

        /// <summary>
        /// Returns a list of the Multi-Geography Regions available for the supplied {org-id
        /// An element is returned for each available Geographic Region.
        /// </summary>
        /// <returns>A list of regions associated with the org ID.</returns>
        public async Task<IReadOnlyCollection<Region>> GetListOfMultiGeographyRegionsAsync()
        {
			CheckDisposed();
			
			var relativeUrl = string.Format("{0}/multigeo", Account.OrganizationId);
            var uri = new Uri(relativeUrl, UriKind.Relative);

            var regions = await WebApi.ApiGetAsync<Geos>(uri);

            return regions.Items;
        }

        /// <summary>
        /// Allows the current Primary Administrator user to designate a Sub-Administrator user belonging to the 
        /// same organization to become the Primary Administrator for the organization.
        /// The Sub-Administrator is identified by their <paramref name="username"/>.
        /// </summary>
        /// <param name="username">The Sub-Administrator account.</param>
        /// <returns>A <see cref="ApiStatus"/> result that describes whether or not the operation was successful.</returns>
		public Task<ApiStatus> DeleteSubAdministratorAccountAsync(string username)
        {
	        if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username cannot be null, Empty or whitespace!", "username");

			CheckDisposed();

			return ExecuteAccountCommandAsync(username, "{0}/account/{1}?delete");
        }

        /// <summary>
        /// Allows the current Primary Administrator user to designate a Sub-Administrator user belonging to the 
        /// same organization to become the Primary Administrator for the organization.
        /// The Sub-Administrator is identified by their <paramref name="username"/>.
        /// </summary>
        /// <param name="username">The Sub-Administrator account.</param>
        /// <returns>A <see cref="ApiStatus"/> result that describes whether or not the operation was successful.</returns>
        public Task<ApiStatus> DesignatePrimaryAdministratorAccountAsync(string username)
        {
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username cannot be null, Empty or whitespace!", "username");

			CheckDisposed();
			
			return ExecuteAccountCommandAsync(username, "{0}/account/{1}?primary");
        }


        /// <summary>
        /// This function identifies the list of data centres available to the organization of the authenticating user. 
        /// </summary>
        /// <returns>The list of data centres associated with the organization.</returns>
        public async Task<IReadOnlyCollection<DatacenterWithMaintenanceStatusType>> GetDataCentersWithMaintenanceStatusesAsync()
        {
			CheckDisposed();
			
			var dataCenters = await WebApi.ApiGetAsync<DatacentersWithMaintenanceStatus>(ApiUris.DatacentresWithMaintanence(Account.OrganizationId));
            return dataCenters.datacenter;
        }

        /// <summary>
        /// Lists the Accounts belonging to the Organization identified by the organisation. The list will include all 
        /// SubAdministrator accounts and the Primary Administrator account. The Primary Administrator is unique and is 
        /// identified by the “primary administrator” role.
        /// </summary>
        /// <returns>A list of accounts associated with the organisation.</returns>
        public async Task<IReadOnlyCollection<Account>> GetAccountsAsync()
        {
			CheckDisposed();
			
			var relativeUrl = string.Format("{0}/account", Account.OrganizationId);
            var accounts = await WebApi.ApiGetAsync<Accounts>(new Uri(relativeUrl, UriKind.Relative));
            return accounts.Items;
        }

        /// <summary>
        /// Adds a new Sub-Administrator Account to the organization. 
        /// The account is created with a set of roles defining the level of access to the organization’s Cloud 
        /// resources or the account can be created as “read only”, restricted to just viewing Cloud resources and 
        /// unable to generate Cloud Reports.
        /// </summary>
        /// <param name="account">The account that will be added to the org.</param>
        /// <returns>A <see cref="Status"/> object instance that shows the results of the operation.</returns>
        public async Task<Status> AddSubAdministratorAccountAsync(Account account)
        {
			if (account == null)
				throw new ArgumentNullException("account", "Account argument cannot be null, Empty or whitespace!");

			CheckDisposed();
			var relativeUrl = string.Format("{0}/account", Account.OrganizationId);

            return await WebApi.ApiPostAsync<Account, Status>(new Uri(relativeUrl, UriKind.Relative), new Account());
        }

        /// <summary>
        /// This function updates an existing Administrator Account.
        /// </summary>
        /// <param name="account">The account to be updated.</param>
        /// <returns>A <see cref="Status"/> object instance that shows the results of the operation.</returns>
        public async Task<Status> UpdateAdministratorAccountAsync(Account account)
        {
			if (account == null)
				throw new ArgumentNullException("account", "Account argument cannot be null, Empty or whitespace!");

			CheckDisposed();
			var parameters = new Dictionary<string, string>();
            parameters["username"] = account.UserName;
            parameters["password"] = account.Password;
            parameters["email"] = account.EmailAddress;
            parameters["fullname"] = account.FullName;
            parameters["firstName"] = account.FirstName;
            parameters["lastName"] = account.LastName;
            parameters["department"] = account.Department;
            parameters["customDefined1"] = account.CustomDefined1;
            parameters["customDefined2"] = account.CustomDefined2;

            var parameterStrings = parameters.Where(kvp => kvp.Value != null).Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value));
            var parameterText = string.Join("&", parameterStrings);

            var roles = account.MemberOfRoles.Select(role => string.Format("role={0}", role.Name));
            var roleParameters = string.Join("&", roles);

            var postBody = string.Join("&", parameterText, roleParameters);

            var relativeUrl = string.Format("{0}/account/{1}", Account.OrganizationId, account.UserName);
            
			return await WebApi.ApiPostAsync<string, Status>(new Uri(relativeUrl, UriKind.Relative), postBody);
        }

		/// <summary>
		/// The execute account command.
		/// </summary>
		/// <param name="username">
		/// The username.
		/// </param>
		/// <param name="uriFormat">
		/// The uri format.
		/// </param>
		/// <returns>
		/// The <see cref="Task"/>.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// </exception>
		private async Task<ApiStatus> ExecuteAccountCommandAsync(string username, string uriFormat)
        {
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username argument cannot be null, Empty or whitespace!", "username");
			if (string.IsNullOrWhiteSpace(uriFormat))
				throw new ArgumentException("Account argument cannot be null, Empty or whitespace!", "uriFormat");

			CheckDisposed();
			var uriText = string.Format(uriFormat, Account.OrganizationId, username);
            var uri = new Uri(uriText, UriKind.Relative);

            return await WebApi.ApiGetAsync<ApiStatus>(uri);
        }

        /// <summary>
        ///		Asynchronously get a list of all CaaS data centres that are available for use by the specified organisation.
        /// </summary>
        /// <returns>
        ///		A read-only list of <see cref="IDatacenterDetail"/>s representing the data centre information.
        /// </returns>
        [Obsolete("This method was replaced by GetListOfDataCentersWithMaintenanceStatuses based on CaaS API!")]
        public async Task<IReadOnlyList<IDatacenterDetail>> GetAvailableDataCentersAsync()
        {
            CheckDisposed();

            DatacentersWithDiskSpeedDetails datacentersWithDiskSpeedDetails =
                await WebApi.ApiGetAsync<DatacentersWithDiskSpeedDetails>(
                    ApiUris.DatacentersWithDiskSpeedDetails(
                        Account.OrganizationId
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
        ///		A read-only list <see cref="DeployedImageWithSoftwareLabelsType"/>, sorted by UTC creation date / time, representing the images.
        /// </returns>
        public async Task<IReadOnlyList<DeployedImageWithSoftwareLabelsType>> GetImagesAsync(string locationName)
        {
            if (String.IsNullOrWhiteSpace(locationName))
                throw new ArgumentException(
                    "Argument cannot be null, empty, or composed entirely of whitespace: 'locationName'.",
                    "locationName");

			CheckDisposed();
			
			var imagesWithSoftwareLabels =
                await
                WebApi.ApiGetAsync<DeployedImagesWithSoftwareLabels>(ApiUris.ImagesWithSoftwareLabels(locationName));

            return imagesWithSoftwareLabels.DeployedImageWithSoftwareLabels;
        }

        /// <summary>
        /// This function lists the available Customer Images at a particular Location for the provided org-id.
        /// The response adds to the deprecated List Deployed Customer Images in Location function with 
        /// the addition of zero to many, optional softwareLabel elements, listing the Priced Software packages installed on the Customer Image.
        /// </summary>
        /// <param name="networkLocation">The network location</param>
        /// <returns>A list of deployed customer images with software labels in location</returns>
        public async Task<IReadOnlyCollection<DeployedImageWithSoftwareLabelsType>> GetCustomerServerImagesAsync(string networkLocation)
        {
			if (string.IsNullOrWhiteSpace(networkLocation))
				throw new ArgumentException("Network argument cannot be null, empty or composed of whitespaces only!", "networkLocation");

			CheckDisposed();

            var images = await WebApi.ApiGetAsync<DeployedImagesWithSoftwareLabels>(ApiUris.CustomerImagesWithSoftwareLabels(Account.OrganizationId, networkLocation));
            return images.DeployedImageWithSoftwareLabels;
        }

        /// <summary>
        /// Deploys a server using an image into a specified network.
        /// </summary>
        /// <param name="name">The name of the server.</param>
        /// <param name="description">The description of the server (optional).</param>
        /// <param name="networkId">The network id</param>
        /// <param name="imageId">The image id</param>
        /// <param name="adminPassword">The administrator password</param>
        /// <param name="isStarted">A value indicating whether the server will be started or not.</param>
        /// <returns>The status of the operation</returns>
	    public async Task<Status> DeployServerImageAsync(string name, string description, string networkId, string imageId, string adminPassword, bool isStarted)
	    {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "name");
			if (string.IsNullOrWhiteSpace(networkId))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "networkId");
			if (string.IsNullOrWhiteSpace(imageId))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "imageId");
			if (string.IsNullOrWhiteSpace(adminPassword))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "adminPassword");

			CheckDisposed();

            return
                await
                this.WebApi.ApiPostAsync<NewServerToDeploy, Status>(
                    ApiUris.DeployServer(Account.OrganizationId),
                    new NewServerToDeploy
                        {
                            name = name,
                            description = description ?? string.Empty,
                            vlanResourcePath =
                                string.Format("/oec/{0}/network/{1}", Account.OrganizationId, networkId),
                            imageResourcePath = string.Format("/oec/base/image/{0}", imageId),
                            administratorPassword = adminPassword,
                            isStarted = isStarted
                        });
	    }

        /// <summary>
        /// Powers on the server.
        /// </summary>
        /// <param name="serverId">The server id</param>
        /// <returns>Returns a status of the HTTP request</returns>
        public async Task<Status> ServerPowerOnAsync(string serverId)
	    {
			if (string.IsNullOrWhiteSpace(serverId))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "serverId");

			CheckDisposed();

			return await WebApi.ApiGetAsync<Status>(ApiUris.PowerOnServer(Account.OrganizationId, serverId));
	    }
        
        /// <summary>
        /// Powers off the server
        /// </summary>
        /// <param name="serverId">The server id</param>
        /// <returns>Returns a status of the HTTP request</returns>
        public async Task<Status> ServerPowerOffAsync(string serverId)
        {
			if (string.IsNullOrWhiteSpace(serverId))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "serverId");
			
			CheckDisposed();

			return await this.WebApi.ApiGetAsync<Status>(ApiUris.PoweroffServer(Account.OrganizationId, serverId));
        }

        /// <summary>
        /// Hard boot of the server.
        /// </summary>
        /// <param name="serverId">The server id</param>
        /// <returns>Returns a status of the HTTP request</returns>
        public async Task<Status> ServerRestartAsync(string serverId)
        {
			if (string.IsNullOrWhiteSpace(serverId))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "serverId");

			CheckDisposed();

			return await this.WebApi.ApiGetAsync<Status>(ApiUris.RebootServer(Account.OrganizationId, serverId));
        }

        /// <summary>
        /// A "Graceful" shutdown of the server.
        /// </summary>
        /// <param name="serverId">The server id</param>
        /// <returns>Returns a status of the HTTP request</returns>
        public async Task<Status> ServerShutdownAsync(string serverId)
        {
			if (string.IsNullOrWhiteSpace(serverId))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "serverId");

			CheckDisposed();

			return await this.WebApi.ApiGetAsync<Status>(ApiUris.ShutdownServer(Account.OrganizationId, serverId));
        }

        /// <summary>
        /// Deletes the server. <remarks>The server must be turned off and with backup disabled</remarks>
        /// </summary>
        /// <param name="serverId">The server id</param>
        /// <returns>Returns a status of the HTTP request</returns>
        public async Task<Status> ServerDeleteAsync(string serverId)
        {
			if (string.IsNullOrWhiteSpace(serverId))
				throw new ArgumentException("argument cannot be null, empty or composed of whitespaces only!", "serverId");

			CheckDisposed();

			return await this.WebApi.ApiGetAsync<Status>(ApiUris.DeleteServer(Account.OrganizationId, serverId));
        }

        /// <summary>
        /// Gets all the deployed servers.
        /// </summary>
        /// <returns>A list of deployed servers</returns>
        public async Task<IReadOnlyCollection<ServerWithBackupType>> GetDeployedServersAsync()
        {
			CheckDisposed();
			
			var servers = await this.WebApi.ApiGetAsync<ServersWithBackup>(ApiUris.DeployedServers(Account.OrganizationId));
            return servers.server;
        }

        #endregion // Public methods
    }
}
