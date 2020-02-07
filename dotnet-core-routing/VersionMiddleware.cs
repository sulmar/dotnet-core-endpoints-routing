using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace dotnet_core_routing
{
    public class VersionMiddleware
    {
        readonly RequestDelegate _next;
        static readonly Assembly _entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        static readonly string _version = FileVersionInfo.GetVersionInfo(_entryAssembly.Location).FileVersion;

        public VersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(_version);
        }
    }
}

