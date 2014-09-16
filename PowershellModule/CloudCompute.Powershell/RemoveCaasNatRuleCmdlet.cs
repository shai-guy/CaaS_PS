﻿namespace DD.CBU.Compute.Powershell
{
    using System;
    using System.Management.Automation;

    using DD.CBU.Compute.Api.Client;
    using DD.CBU.Compute.Api.Client.Network;

    /// <summary>
    /// The Remove NAT Rule cmdlet.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "CaasNatRule")]
    public class RemoveCaasNatRuleCmdlet : PsCmdletCaasBase
    {
        [Parameter(Mandatory = true, HelpMessage = "The network that the ACL Rule exists")]
        public NetworkWithLocationsNetwork Network { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "The ACL rule to delete", ValueFromPipeline = true)]
        public NatRuleType NatRule { get; set; }

        /// <summary>
        /// The process record method.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                DeleteNatRule();
            }
            catch (AggregateException ae)
            {
                ae.Handle(
                    e =>
                        {
                            if (e is ComputeApiException)
                            {
                                WriteError(new ErrorRecord(e, "-2", ErrorCategory.InvalidOperation, CaaS));
                            }
                            else //if (e is HttpRequestException)
                            {
                                ThrowTerminatingError(new ErrorRecord(e, "-1", ErrorCategory.ConnectionError, CaaS));
                            }
                            return true;
                        });
            }
        }

        /// <summary>
        /// Removes a NAT Rule
        /// </summary>
        private void DeleteNatRule()
        {
            var status = CaaS.ApiClient.DeleteNatRule(Network.id, NatRule.id).Result;
            if (status != null)
            {
                WriteDebug(
                    string.Format(
                        "{0} resulted in {1} ({2}): {3}",
                        status.operation,
                        status.result,
                        status.resultCode,
                        status.resultDetail));
            }
        }
    }
}