﻿using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;

namespace Library.API.Helpers
{
	[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
	public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
	{
		private readonly string _requestHeaderToMatch;
		private readonly string[] _mediaTypes;

		public int Order
		{
			get
			{
				return 0;
			}
		}

		public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch, string[] mediaTypes)
		{
			_requestHeaderToMatch = requestHeaderToMatch;
			_mediaTypes = mediaTypes;
		}

		public bool Accept(ActionConstraintContext context)
		{
			var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
			if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
				return false;

			foreach (var mediaType in _mediaTypes)
			{
				var mediaTypeMatches = string.Equals(requestHeaders[_requestHeaderToMatch].ToString(), mediaType, StringComparison.OrdinalIgnoreCase);
				if (mediaTypeMatches)
					return true;
			}
			return false;
		}
	}
}
