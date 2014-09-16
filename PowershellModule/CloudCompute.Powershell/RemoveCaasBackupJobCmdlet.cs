﻿namespace DD.CBU.Compute.Powershell
{
    using System;
    using System.Globalization;
    using System.Management.Automation;

    using DD.CBU.Compute.Api.Client;
    using DD.CBU.Compute.Api.Client.Backup;

    /// <summary>
    /// The Remove-Backup now job cmdlet.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "CaasBackupJob")]
    [OutputType(typeof(ServerWithBackupType))]
    public class RemoveCaasBackupJobCmdlet : PsCmdletCaasBase
    {
        [Parameter(Mandatory = true, HelpMessage = "The server to modify the backup client",
            ValueFromPipeline = true)]
        public ServerWithBackupType Server { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "The backup client details to modify")]
        public BackupClientDetailsType BackupClient { get; set; }

        /// <summary>
        /// The process record method.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                RemoveBackupJob();
                WriteObject(Server);
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
        /// Modify a backup client
        /// </summary>
        private void RemoveBackupJob()
        {
            var status = CaaS.ApiClient.CancelBackupJob(Server.id, BackupClient).Result;
            if (status != null)
            {
                WriteDebug(
                    string.Format(CultureInfo.CurrentCulture,
                        "{0} resulted in {1} ({2}): {3}",
                        status.operation,
                        status.result,
                        status.resultCode,
                        status.resultDetail));
            }
        }
    }
}