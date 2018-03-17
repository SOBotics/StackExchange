﻿using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace StackExchange.Chat.Actions.Room
{
	public class Timeout : ChatAction
	{
		internal override Method RequestMethod => Method.POST;

		internal override string Endpoint => $"https://{Host}/rooms/timeout/{RoomId}";

		internal override bool RequiresFKey => true;

		internal override bool RequiresAuthCookies => true;

		internal override ActionPermissionLevel RequiredPermissionLevel => ActionPermissionLevel.RoomOwner;



		public Timeout(int durationSeconds, string reason)
		{
			Data = new Dictionary<string, object>
			{
				["duration"] = durationSeconds,
				["reason"] = reason
			};
		}



		internal override object ProcessResponse(HttpStatusCode status, string response)
		{
			return status == HttpStatusCode.OK &&
				response?.ToUpperInvariant() == "TIMEOUT APPLIED";
		}
	}
}
