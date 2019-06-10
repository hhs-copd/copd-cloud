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

                if (string.IsNullOrEmpty(file.FileName) || string.IsNullOrEmpty(file.Content))
                {
                    return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest,
                        "Request doesnt contain filename or content in JSON");
                }

                if (!request.Headers.TryGetValue("Authorization", out var header))
                {
                    return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest,
                        "No authorization header present, check request or mapping template");
                }

                var jwt = new JwtSecurityToken(header.Replace("Bearer ", string.Empty));

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
                            {"Date", new AttributeValue {N = itemGroup.First().DateTime.ToUnixTimeSeconds().ToString()}}
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
    }
}