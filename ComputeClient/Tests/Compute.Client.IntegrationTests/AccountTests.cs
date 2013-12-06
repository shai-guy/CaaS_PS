using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DD.CBU.Compute.Client.IntegrationTests
{
	using Api.Client;
	using Api.Contracts.Directory;

	/// <summary>
	///		Unit-tests for <see cref="Account"/>-related CaaS API functionality.
	/// </summary>
	[TestClass]
	public class AccountTests
	{
		/// <summary>
		///		Create a new Account unit-test set.
		/// </summary>
		public AccountTests()
		{	
		}

		/// <summary>
		///		The unit-test execution context.
		/// </summary>
		public TestContext TestContext
		{
			get;
			set;
		}

		/// <summary>
		///		Attempt to log in with valid credentials.
		/// </summary>
		/// <returns>
		///		A <see cref="Task"/> representing the asynchronous unit-test execution.
		/// </returns>
		[TestMethod]
		public async Task LoginWithValidCredentials()
		{
			ICredentials credentials = GetIntegrationTestCredentials();
			using (ComputeApiClient computeApiClient = new ComputeApiClient("au"))
			{
				IAccount account = await
					computeApiClient
						.LoginAsync(credentials);
				Assert.IsNotNull(account);
				Assert.IsNotNull(computeApiClient.Account);

				TestContext.WriteLine("Account organisation Id: '{0}", account.OrganizationId);
				TestContext.WriteLine("Account full name: '{0}'", account.FullName);
				TestContext.WriteLine("Account email address: '{0}'", account.EmailAddress);
			}
		}

		/// <summary>
		///		Get CaaS account credentials for use during integration tests.
		/// </summary>
		/// <returns>
		///		An <see cref="ICredentials"/> implementation representing the credentials to use when authenticating to the API.
		/// </returns>
		/// <remarks>
		///		Since this is an open-source project, I can't really supply valid credentials as part of the source code (so we just get them from the registry).
		/// </remarks>
		static ICredentials GetIntegrationTestCredentials()
		{
			const string credentialsKeyName = @"Software\Dimension Data\Cloud\API Client\Credentials\Test1";
			using (RegistryKey credentialsKey = Registry.CurrentUser.OpenSubKey(credentialsKeyName))
			{
				if (credentialsKey == null)
				{
					throw new InvalidOperationException(
						String.Format(
							"Cannot find credentials under registry key HKCU\\{0}.",
							credentialsKeyName
						)
					);
				}

				string userName = (string)credentialsKey.GetValue("Login");
				if (String.IsNullOrWhiteSpace(userName))
					throw new InvalidOperationException(
						String.Format(
							"Cannot find 'Login' value under registry key HKCU\\{0}.",
							credentialsKeyName
						)
					);
				
				string password = (string)credentialsKey.GetValue("Password");
				if (String.IsNullOrWhiteSpace(password))
					throw new InvalidOperationException(
						String.Format(
							"Cannot find 'Password' value under registry key HKCU\\{0}.",
							credentialsKeyName
						)
					);

				return new NetworkCredential(userName, password);
			}
		}
	}
}
