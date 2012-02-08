﻿//-----------------------------------------------------------------------
// <copyright file="IReplayProtectedProtocolMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Kiss.Web.Auth.Messaging.Bindings {
	using System;
	using Kiss.Web.Auth.Messaging;

	/// <summary>
	/// The contract a message that has an allowable time window for processing must implement.
	/// </summary>
	/// <remarks>
	/// All replay-protected messages must also be set to expire so the nonces do not have
	/// to be stored indefinitely.
	/// </remarks>
	internal interface IReplayProtectedProtocolMessage : IExpiringProtocolMessage, IDirectedProtocolMessage {
		/// <summary>
		/// Gets the context within which the nonce must be unique.
		/// </summary>
		/// <value>
		/// The value of this property must be a value assigned by the nonce consumer
		/// to represent the entity that generated the nonce.  The value must never be
		/// <c>null</c> but may be the empty string.
		/// This value is treated as case-sensitive.
		/// </value>
		string NonceContext { get; }

		/// <summary>
		/// Gets or sets the nonce that will protect the message from replay attacks.
		/// </summary>
		string Nonce { get; set; }
	}
}
