﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using StackExchange.Auth;
using StackExchange.Net;

namespace StackExchange.Chat.Actions
{
	public abstract class ChatAction
	{
		internal Action<object> CallBack { get; set; }

		internal string Host { get; set; }

		internal int RoomId { get; set; }

		internal abstract Method RequestMethod { get; }

		internal abstract string Endpoint { get; }

		internal abstract bool RequiresFKey { get; }

		internal abstract bool RequiresAuthCookies { get; }

		internal abstract ActionPermissionLevel RequiredPermissionLevel { get; }

		public Dictionary<string, object> Data { get; set; }



		internal abstract object ProcessResponse(string response);
	}
}
