﻿using Microsoft.AspNetCore.Http;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = await request.Content.CloneAsync(),
                Version = request.Version
            };
            foreach (var prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        public static async Task<HttpContent> CloneAsync(this HttpContent content)
        {
            if (content == null)
            {
                return null;
            }

            var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            ms.Position = 0;

            var clone = new StreamContent(ms);
            foreach (var header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }
            return clone;
        }
    }
}
