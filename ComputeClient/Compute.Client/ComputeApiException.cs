using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DD.CBU.Compute.Api.Client
{
	/// <summary>
	///		Exception raised by the CaaS API client when it encounters an error.
	/// </summary>
	public class ComputeApiException
		: Exception
	{
		/// <summary>
		///		The reason that the <see cref="ComputeApiException"/> was raised.
		/// </summary>
		readonly ClientError _reason;

		/// <summary>
		///		Create a new <see cref="ComputeApiException"/>.
		/// </summary>
		/// <param name="reason">
		///		The reason that the exception is being raised.
		/// </param>
		/// <param name="messageOrFormat">
		///		The exception message or message format.
		/// </param>
		/// <param name="formatArguments">
		///		Optional message format arguments.
		/// </param>
		public ComputeApiException(ClientError reason, string messageOrFormat, params object[] formatArguments)
			: base(String.Format(messageOrFormat, formatArguments))
		{
			Debug.Assert(reason != ClientError.Unknown, "Reason.Unknown should not be used here.");
			Debug.Assert(String.IsNullOrWhiteSpace(messageOrFormat), "Exception message should not be empty.");

			_reason = reason;
		}

		/// <summary>
		///		Create a new <see cref="ComputeApiException"/>.
		/// </summary>
		/// <param name="reason">
		///		The reason that the exception is being raised.
		/// </param>
		/// <param name="messageOrFormat">
		///		The exception message or message format.
		/// </param>
		/// <param name="innerException">
		///		A previous exception that caused the current exception to be raised.
		/// </param>
		/// <param name="formatArguments">
		///		Optional message format arguments.
		/// </param>
		public ComputeApiException(ClientError reason, string messageOrFormat, Exception innerException, params object[] formatArguments)
			: base(String.Format(messageOrFormat, formatArguments), innerException)
		{
			Debug.Assert(reason != ClientError.Unknown, "Reason.Unknown should not be used here.");
			Debug.Assert(String.IsNullOrWhiteSpace(messageOrFormat), "Exception message should not be empty.");

			_reason = reason;
		}

		/// <summary>
		///		Deserialisation constructor for <see cref="ComputeApiException"/>.
		/// </summary>
		/// <param name="info">
		///		A <see cref="SerializationInfo"/> serialisation data store that holds the serialized exception data.
		/// </param>
		/// <param name="context">
		///		A <see cref="StreamingContext"/> value that indicates the source of the serialised data.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		The <paramref name="info"/> parameter is null.
		/// </exception>
		/// <exception cref="SerializationException">
		///		The class name is <c>null</c> or <see cref="Exception.HResult"/> is zero (0).
		/// </exception>
		protected ComputeApiException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			_reason = (ClientError)info.GetValue("_reason", typeof(ClientError));
		}

		/// <summary>
		///		The reason that the <see cref="ComputeApiException"/> was raised.
		/// </summary>
		public ClientError Reason
		{
			get
			{
				return _reason;
			}
		}

		/// <summary>
		///		Get exception data for serialisation.
		/// </summary>
		/// <param name="info">
		///		A <see cref="SerializationInfo"/> serialisation data store that will hold the serialized exception data.
		/// </param>
		/// <param name="context">
		///		A <see cref="StreamingContext"/> value that indicates the source of the serialised data.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		The <paramref name="info"/> parameter is null.
		/// </exception>
		/// <exception cref="SerializationException">
		///		The class name is <c>null</c> or <see cref="Exception.HResult"/> is zero (0).
		/// </exception>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			info.AddValue("_reason", _reason);

			base.GetObjectData(info, context);
		}

		#region Reason enum

		/// <summary>
		///		Represents the reason that a <see cref="ComputeApiException"/> was raised.
		/// </summary>
		public enum ClientError
		{
			/// <summary>
			///		An unknown reason.
			/// </summary>
			/// <remarks>
			///		Used to detect uninitialised values; do not use directly.
			/// </remarks>
			Unknown = 0,

			/// <summary>
			///		The client has not logged into the CaaS API.
			/// </summary>
			/// <remarks>
			///		Call <see cref="ComputeApiClient.LoginAsync"/> before calling other operations.
			/// </remarks>
			NotLoggedIn		= 1,

			/// <summary>
			///		The client is already logged into the CaaS API.
			/// </summary>
			/// <remarks>
			///		To log in with different credentials than those currently used by the client, first call <see cref="ComputeApiClient.Logout"/> before calling <see cref="ComputeApiClient.LoginAsync"/>.
			/// </remarks>
			AlreadyLoggedIn	= 2
		}

		#endregion // Reason enum

		#region Factory methods

		/// <summary>
		///		Create a <see cref="ComputeApiException"/> to be raised because the client is not currently logged into the CaaS API.
		/// </summary>
		/// <returns>
		///		The configured <see cref="ComputeApiException"/>.
		/// </returns>
		public static ComputeApiException NotLoggedIn()
		{
			return new ComputeApiException(
				ClientError.NotLoggedIn,
				"The client is not currently logged into the CaaS API (call LoginAsync, first)."
			);
		}

		/// <summary>
		///		Create a <see cref="ComputeApiException"/> to be raised because the client is already logged into the CaaS API.
		/// </summary>
		/// <returns>
		///		The configured <see cref="ComputeApiException"/>.
		/// </returns>
		public static ComputeApiException AlreadyLoggedIn()
		{
			return new ComputeApiException(
				ClientError.AlreadyLoggedIn,
				"The client is already logged into the CaaS API (call Logout, first)."
			);
		}

		#endregion // Factory methods
	}
}
