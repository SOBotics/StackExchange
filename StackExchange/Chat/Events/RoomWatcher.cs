﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using StackExchange.Auth;
using StackExchange.Net;
using StackExchange.Net.WebSockets;

namespace StackExchange.Chat.Events
{
	public class RoomWatcher<T> where T : IWebSocket
	{
		private CookieManager cMan => new CookieManager(AuthCookies);

		public string Host { get; private set; }

		public int RoomId { get; private set; }

		public ReadOnlyCollection<Cookie> AuthCookies { get; private set; }

		public EventRouter EventRouter { get; private set; }
		
		public IWebSocket WebSocket { get; private set; }



		public RoomWatcher(IEnumerable<Cookie> authCookies, string roomUrl)
		{
			if (authCookies.Count() == 0)
			{
				throw new ArgumentException($"'{nameof(authCookies)}' cannot be empty.", nameof(authCookies));
			}

			if (string.IsNullOrEmpty(roomUrl))
			{
				throw new ArgumentException($"'{nameof(roomUrl)}' cannot be null or empty.", nameof(roomUrl));
			}

			if (authCookies == null)
			{
				throw new ArgumentNullException(nameof(authCookies));
			}

			roomUrl.GetHostAndIdFromRoomUrl(out var host, out var roomId);

			Initialise(authCookies, host, roomId);
		}

		public RoomWatcher(IEnumerable<Cookie> authCookies, string host, int roomId)
		{
			if (authCookies == null)
			{
				throw new ArgumentNullException(nameof(authCookies));
			}

			if (authCookies.Count() == 0)
			{
				throw new ArgumentException($"'{nameof(authCookies)}' cannot be empty.", nameof(authCookies));
			}

			if (string.IsNullOrEmpty(host))
			{
				throw new ArgumentException($"'{nameof(host)}' cannot be null or empty.");
			}

			if (roomId < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(roomId), $"'{nameof(roomId)}' cannot be negative.");
			}

			Initialise(authCookies, host, roomId);
		}



		private void Initialise(IEnumerable<Cookie> authCookies, string host, int roomId)
		{
			var cookies = new List<Cookie>(authCookies);

			AuthCookies = new ReadOnlyCollection<Cookie>(cookies);
			Host = host;
			RoomId = roomId;

			WebSocket = GetWebSocket();
			EventRouter = new EventRouter(roomId, WebSocket);
		}

		private IWebSocket GetWebSocket()
		{
			var wsType = typeof(T);
			var ws = (IWebSocket)Activator.CreateInstance(wsType);
			var url = GetWebSocketUrl();

			ws.Connect(url, $"https://{Host}");

			return ws;
		}

		private string GetWebSocketUrl()
		{
			var baseUrl = GetBaseWebSocketUrl();
			var eventCount = GetGlobalEventCount();

			return $"{baseUrl}?l={eventCount}";
		}

		private string GetBaseWebSocketUrl()
		{
			var response = new HttpRequest
			{
				Verb = RestSharp.Method.POST,
				Endpoint = $"https://{Host}/ws-auth",
				Cookies = cMan,
				Data = new Dictionary<string, object>
				{
					["roomid"] = RoomId,
					["fkey"] = FKeyAccessor.Get($"https://{Host}/rooms/{RoomId}", cMan)
				}
			}.Send();

			if (response.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new Exception("Failed to get base WebSocket URL.");
			}

			dynamic data = JObject.Parse(response.Content);

			return data.url;
		}

		private int GetGlobalEventCount()
		{
			var response = new HttpRequest
			{
				Verb = RestSharp.Method.POST,
				Endpoint = $"https://{Host}/chats/{RoomId}/events",
				Cookies = cMan,
				Data = new Dictionary<string, object>
				{
					["mode"] = "events",
					["msgCount"] = 0,
					["fkey"] = FKeyAccessor.Get($"https://{Host}/rooms/{RoomId}", cMan)
				}
			}.Send();

			if (response.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new Exception("Failed to get global chat event count.");
			}

			dynamic data = JObject.Parse(response.Content);

			return data.time;
		}
	}
}