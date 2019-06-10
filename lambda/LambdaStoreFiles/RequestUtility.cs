using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mime;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Util;
using Newtonsoft.Json;

namespace LambdaStoreFiles
{
    public static class RequestUtility
    {
        public static string GetUserId(IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                return null;
            }

            string authorizationHeader = headers[HeaderKeys.AuthorizationHeader];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return null;
            }

            try
            {
                string strippedRawToken = authorizationHeader.Replace("Bearer ", "");
                var token = new JwtSecurityToken(strippedRawToken);
                return token.Subject;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static APIGatewayProxyResponse SuccessResponse(object data, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            bool isPlain = data is string;
            string body = isPlain ? (string) data : JsonConvert.SerializeObject(data);
            string contentType = isPlain ? MediaTypeNames.Text.Plain : MediaTypeNames.Application.Json;

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) statusCode,
                Body = body,
                Headers = new Dictionary<string, string> {{ HeaderKeys.ContentTypeHeader, contentType }}
            };
        }

        public static APIGatewayProxyResponse ErrorResponse(HttpStatusCode statusCode, string error)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int) statusCode,
                Body = error,
                Headers = new Dictionary<string, string> {{ HeaderKeys.ContentTypeHeader, MediaTypeNames.Text.Plain }}
            };
        }
    }
}