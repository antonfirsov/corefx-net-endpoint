using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreServer
{
    public class TrailerHandler
    {
        public static async Task InvokeAsync(HttpContext context)
        {
            if (!context.Response.SupportsTrailers())
            {
                await context.Response.WriteAsync(context.Features.GetType().FullName);
                return;
            }

            context.Response.Headers.Add("Connection", "close");

            context.Response.DeclareTrailer("MyCoolTrailerHeader");
            context.Response.DeclareTrailer("EmptyHeader");
            context.Response.DeclareTrailer("Accept-Encoding");
            context.Response.DeclareTrailer("Hello");

            await context.Response.Body.WriteAsync(Encoding.ASCII.GetBytes("data"));

            context.Response.AppendTrailer("MyCoolTrailerHeader", "amazingtrailer");
            context.Response.AppendTrailer("EmptyHeader", "");
            context.Response.AppendTrailer("Accept-Encoding", "identity,gzip");
            context.Response.AppendTrailer("Hello", "World");
        }
    }
}
