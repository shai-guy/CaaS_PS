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
		/// <param name="targetRegionName">
		///		The target region's short name ("au", for example).
		/// </param>
		/// <returns>
		///		The base URI for the CaaS REST API.
		/// </returns>
		public static Uri ComputeBase(string targetRegionName)
		{
			if (String.IsNullOrWhiteSpace(targetRegionName))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'targetRegionName'.", "targetRegionName");

			return new Uri(
				String.Format(
					"https://api-{0}.dimensiondata.com/oec/0.9/",
					targetRegionName.ToLower()
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

		/// <summary>
		///		Get the relative URI for the CaaS API action that retrieves a list of all system-defined images deployed in the specified data centre.
		/// </summary>
		/// <param name="locationName">
		///		The data centre location name.
		/// </param>
		/// <returns>
		///		The relative action Uri.
		/// </returns>
		public static Uri ImagesWithSoftwareLabels(string locationName)
		{
			if (String.IsNullOrWhiteSpace(locationName))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'locationName'.", "locationName");

			return new Uri(
				String.Format(
					"base/image/deployedWithSoftwareLabels/{0}",
					locationName
				),
				UriKind.Relative
			);
		}
	}
}
