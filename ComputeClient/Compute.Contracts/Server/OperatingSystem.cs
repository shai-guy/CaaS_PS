using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DD.CBU.Compute.Api.Contracts.Server
{
	using System.Xml.Serialization;

	/// <summary>
	///		Represents a well-known operating system for CaaS virtual machines.
	/// </summary>
	public class OperatingSystem
	{
		/// <summary>
		///		Create a new operating system.
		/// </summary>
		public OperatingSystem()
		{
		}

		/// <summary>
		///		The operating system type.
		/// </summary>
		[XmlElement("type", Namespace = XmlNamespaceConstants.Server)]
		public OperatingSystemType OperatingSystemType
		{
			get;
			set;
		}

		/// <summary>
		///		The operating system display-name.
		/// </summary>
		[XmlElement("displayName", Namespace = XmlNamespaceConstants.Server)]
		public string DisplayName
		{
			get;
			set;
		}
	}
}
