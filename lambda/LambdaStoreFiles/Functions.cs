using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LambdaStoreFiles.CSV;

namespace LambdaStoreFiles
{
    public class Functions
    {
        private const string TableName = "Data";
        private static readonly RegionEndpoint RegionEndpoint = RegionEndpoint.EUCentral1;

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public static async Task<APIGatewayProxyResponse> PostData(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            try
            {
                var file = JsonConvert.DeserializeObject<FileContent>(request.Body);

                if (string.IsNullOrEmpty(file.Content))
                {
                    return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest,
                        "Request doesnt contain filename or content in JSON");
                }

                if (!request.Headers.TryGetValue("Authorization", out var authorizationHeader))
                {
                    return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest,
                        "No authorization header present, check request or mapping template");
                }

                var jwt = new JwtSecurityToken(authorizationHeader.Replace("Bearer ", string.Empty));

                var itemGroups = file
                    .Content
                    .Split('\n')
                    .Select(CSVParser.Parse)
                    .GroupBy(e => e.DateTime);

                using (var client = new AmazonDynamoDBClient(RegionEndpoint))
                {
                    foreach (var itemGroup in itemGroups)
                    {
                        var data = new Dictionary<string, AttributeValue>
                        {
                            {"Id", new AttributeValue {S = Guid.NewGuid().ToString()}},
                            {"UserId", new AttributeValue {S = jwt.Subject}},
                            {
                                "Date",
                                new AttributeValue {N = itemGroup.First().DateTime.ToUnixTimeMilliseconds().ToString()}
                            }
                        };

                        foreach (var item in itemGroup)
                        {
                            if (!data.ContainsKey(item.Name))
                            {
                                data.Add(item.Name, item.Value);
                            }
                        }

                        await client.PutItemAsync(TableName, data);
                    }
                }

                return RequestUtility.SuccessResponse("success", HttpStatusCode.Created);
            }
            catch (Exception exception)
            {
                return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public static async Task<APIGatewayProxyResponse> GetData(APIGatewayProxyResponse request,
            ILambdaContext context)
        {
            if (!request.Headers.TryGetValue("X-Value", out var valueHeader))
            {
                return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest,
                    "No X-Value header present, we need this for getting the correct graph");
            }

            if (!request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest,
                    "No authorization header present, check request or mapping template");
            }

            var jwt = new JwtSecurityToken(authorizationHeader.Replace("Bearer ", string.Empty));

            using (var client = new AmazonDynamoDBClient(RegionEndpoint))
            {
                var scanRequest = new ScanRequest(TableName)
                {
                    ConsistentRead = true,
                    FilterExpression = "attribute_exists(UserId) AND (UserId = :userId)",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        {"#date", "Date"},
                        {"#user", "UserId"}
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":userId", new AttributeValue {S = jwt.Subject}}
                    },
                    ProjectionExpression = "#user, #date, " + valueHeader
                };

                var result = await client.ScanAsync(scanRequest);

                return RequestUtility.SuccessResponse(
                    result
                        .Items
                        .Select(item =>
                        {
                            if (item.TryGetValue(valueHeader, out var value) && item.TryGetValue("Date", out var date))
                            {
                                return new Dictionary<string, string>
                                {
                                    {"x", date.N},
                                    {
                                        "y", value.N ?? value.S ?? (value.NS != null
                                                 ? string.Join(',', value.NS)
                                                 : value.SS != null
                                                     ? string.Join(',', value.SS)
                                                     : null)
                                    },
                                };
                            }

                            return null;
                        })
                        .Where(e => e != null));
            }
        }
    }
}