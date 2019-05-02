using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Util;
using Newtonsoft.Json;
using DynamoDBContextConfig = Amazon.DynamoDBv2.DataModel.DynamoDBContextConfig;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace LambdaUserStore
{
    public class Functions
    {
        // This const is the name of the environment variable that the serverless.template will use to set
        // the name of the DynamoDB table used to store blog posts.
        private const string TableNameEnvironmentVariableLookup = "DYNAMO_TABLE_NAME";

        // This const is the name of the column in DynamoDB
        public const string UserIdQueryStringName = "UserId";

        private readonly DynamoDBContextConfig _dynamoDbContextConfig = new DynamoDBContextConfig {Conversion = DynamoDBEntryConversion.V2};

        /// <summary>
        ///     Default constructor that Lambda will invoke.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public Functions()
        {
            string tableName = Environment.GetEnvironmentVariable(TableNameEnvironmentVariableLookup);

            AddTypeMappings(tableName);

            this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), _dynamoDbContextConfig);
        }

        /// <summary>
        ///     Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName">Name of the table in DynamoDB</param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            AddTypeMappings(tableName);

            this.DDBContext = new DynamoDBContext(ddbClient, _dynamoDbContextConfig);
        }

        private static void AddTypeMappings(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                return;
            }

            AWSConfigsDynamoDB.Context.TypeMappings[typeof(DataEntry)] = new TypeMapping(typeof(DataEntry), tableName);
        }

        private IDynamoDBContext DDBContext { get; }

        /// <summary>
        ///     A Lambda function that returns back a page worth of data entries.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>The list of data entries</returns>
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> GetDataEntriesAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string userId = RequestUtility.GetUserId(request.Headers);

            context.Logger.LogLine("Getting entries");
            try
            {
                AsyncSearch<DataEntry> search = this.DDBContext.ScanAsync<DataEntry>(new[]
                    {new ScanCondition("UserId", ScanOperator.Equal, userId)});
                List<DataEntry> page = await search.GetNextSetAsync();
                context.Logger.LogLine($"Found {page.Count} entries");

                return RequestUtility.SuccessResponse(page);
            }
            catch (Exception exception)
            {
                context.Logger.LogLine("Something went wrong fetching data: " + exception);
                return RequestUtility.ErrorResponse(HttpStatusCode.InternalServerError, "Something went wrong while trying to fetch the data");
            }
        }

        /// <summary>
        ///     A Lambda function that adds a data entry
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A success/failure message</returns>
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> AddDataAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string userId = RequestUtility.GetUserId(request.Headers);
            if (userId == null)
            {
                context.Logger.LogLine($"Bad Request: User got through authentication with invalid authorization header: {request.Headers[HeaderKeys.AuthorizationHeader]}");
                return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest, "Must supply valid user token with request");
            }

            context.Logger.LogLine($"Saving data entries for user {userId}");

            try
            {
                IEnumerable<DataEntry> entries = JsonConvert
                    .DeserializeObject<IEnumerable<DataEntry>>(request.Body)
                    .Select(entry =>
                    {
                        entry.UserId = userId;
                        return entry;
                    });

                BatchWrite<DataEntry> batch =
                    this.DDBContext.CreateBatchWrite<DataEntry>(new DynamoDBOperationConfig());
                batch.AddPutItems(entries);
                await this.DDBContext.SaveAsync(batch);

                return RequestUtility.SuccessResponse("Saved data", HttpStatusCode.Created);
            }
            catch (Exception exception)
            {
                context.Logger.LogLine("Something went wrong storing data: " + exception);
                return RequestUtility.ErrorResponse(HttpStatusCode.InternalServerError, "Something went wrong while trying to store the data");
            }
        }
    }
}