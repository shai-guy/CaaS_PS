using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace DD.CBU.Compute.Powershell
{
	/// <summary>
	///		The "New-ComputeServiceConnection" Cmdlet.
	/// </summary>
	/// <remarks>
	///		Used to create a new connection to the CaaS API.
	/// </remarks>
	[Cmdlet("New", "ComputeServiceConnection", SupportsShouldProcess = true)]
	public class NewComputeServiceConnectionCmdlet
		: Cmdlet
	{
		/// <summary>
		///		Create a new <see cref="NewComputeServiceConnectionCmdlet"/>.
		/// </summary>
		public NewComputeServiceConnectionCmdlet()
		{
		}

		/// <summary>
		///		The name of the region, to whose CaaS API, the connection should be created.
		/// </summary>
		[Parameter(Mandatory = true)]
		public string RegionName
		{
			get;
			set;
		}

		/// <summary>
		///		The credentials used to connect to the CaaS API.
		/// </summary>
		[Parameter(Mandatory = true, ValueFromPipeline = true)]
		public PSCredential ApiCredentials
		{
			get;
			set;
		}
	}
}
