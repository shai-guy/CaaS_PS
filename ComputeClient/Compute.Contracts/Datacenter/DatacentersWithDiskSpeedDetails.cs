﻿using System.Collections.Generic;
using System.Xml.Serialization;

namespace DD.CBU.Compute.Api.Contracts.Datacenter
{
	/// <summary>
	///		An XML-serialisable data contract that represents a list of data centres with disk speed details, returned by the CaaS API.
	/// </summary>
	[XmlRoot("DatacentersWithDiskSpeedDetails", Namespace = XmlNamespaceConstants.Datacenter)]
	public class DatacentersWithDiskSpeedDetails
	{
		/// <summary>
		///		The data centres.
		/// </summary>
		readonly List<DatacenterWithDiskSpeedDetail> _datacenters = new List<DatacenterWithDiskSpeedDetail>();

		/// <summary>
		///		Create a new <see cref="DatacentersWithDiskSpeedDetails"/> data contract.
		/// </summary>
		public DatacentersWithDiskSpeedDetails()
		{
		}

		/// <summary>
		///		The data centres (with disk speed details).
		/// </summary>
		[XmlElement("datacenter")]
		public List<DatacenterWithDiskSpeedDetail> Datacenters
		{
			get
			{
				return _datacenters;
			}
		}
	}
}
