using System;

namespace DD.CBU.Compute.Api.Client
{
	using Contracts.Datacenter;

	/// <summary>
	///		Constants and formatters for API URLs.
	/// </summary>
	public static class ApiUris
	{
		/// <summary>
		///		The path (relative to the base API URL) of the My Account action.
		/// </summary>
		public static readonly Uri MyAccount = new Uri("myaccount", UriKind.Relative);

		/// <summary>
		///		Get the base URI for the CaaS REST API.
		/// </summary>
		/// <param name="regionCode">
		///		The region's short name ("au", for example).
		/// </param>
		/// <returns>
		///		The base URI for the CaaS REST API.
		/// </returns>
		public static Uri ComputeBase(string regionCode)
		{
			if (String.IsNullOrWhiteSpace(regionCode))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'regionCode'.", "regionCode");

			return new Uri(
				String.Format(
					"https://api-{0}.dimensiondata.com/oec/0.9/",
					regionCode
				)
			);
		}

		/// <summary>
		///		Get the relative URI for the CaaS API action that retrieves a list of all data centres available for use by the specified organisation.
		/// </summary>
		/// <param name="organizationId">
		///		The organisation Id.
		/// </param>
		/// <returns>
		///		The relative action Uri.
		/// </returns>
		public static Uri DatacentersWithDiskSpeedDetails(Guid organizationId)
		{
			if (organizationId == Guid.Empty)
				throw new ArgumentException("GUID cannot be empty: 'organizationId'.", "organizationId");

			return new Uri(
				String.Format("{0}/datacenterWithDiskSpeed", organizationId),
				UriKind.Relative
			);
		}
	}
}
