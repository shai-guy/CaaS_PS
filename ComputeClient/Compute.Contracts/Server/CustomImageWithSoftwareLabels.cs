using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DD.CBU.Compute.Api.Contracts.Server
{
	/// <summary>
	///		Represents a user-created CaaS image with software labels.
	/// </summary>
	[XmlRoot("ImageWithSoftwareLabels", Namespace = XmlNamespaceConstants.Server)]
	public class CustomImageWithSoftwareLabels
		: ImageWithSoftwareLabels
	{
		/// <summary>
		///		Create a new <see cref="CustomImageWithSoftwareLabels"/>.
		/// </summary>
		public CustomImageWithSoftwareLabels()
		{
		}

		/// <summary>
		///		The Id of the server from which the image was created.
		/// </summary>
		[XmlElement("sourceServerId", Namespace = XmlNamespaceConstants.Server)]
		public Guid? SourceServerId
		{
			get;
			set;
		}

		// TODO: Capture custom image status.
	}
}
