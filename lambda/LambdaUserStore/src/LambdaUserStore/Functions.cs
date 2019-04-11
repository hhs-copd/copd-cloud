using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
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
        public const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "BlogTable";

        public const string ID_QUERY_STRING_NAME = "Id";

        /// <summary>
        ///     Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            // Check to see if a table name was passed in through environment variables and if so
            // add the table mapping.
            string tableName = Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            if (!string.IsNullOrEmpty(tableName))
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Blog)] =
                    new TypeMapping(typeof(Blog), tableName);

            DynamoDBContextConfig config = new DynamoDBContextConfig {Conversion = DynamoDBEntryConversion.V2};
            this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        /// <summary>
        ///     Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Blog)] =
                    new TypeMapping(typeof(Blog), tableName);

            DynamoDBContextConfig config = new DynamoDBContextConfig {Conversion = DynamoDBEntryConversion.V2};
            this.DDBContext = new DynamoDBContext(ddbClient, config);
        }

        private IDynamoDBContext DDBContext { get; }

        /// <summary>
        ///     A Lambda function that returns back a page worth of blog posts.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> GetBlogsAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Getting blogs");
            AsyncSearch<Blog> search = this.DDBContext.ScanAsync<Blog>(null);
            List<Blog> page = await search.GetNextSetAsync();
            context.Logger.LogLine($"Found {page.Count} blogs");

            APIGatewayProxyResponse response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(page),
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };

            return response;
        }

        /// <summary>
        ///     A Lambda function that returns the blog identified by blogId
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> GetBlogAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string blogId = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
                blogId = request.PathParameters[ID_QUERY_STRING_NAME];
            else if (request.QueryStringParameters != null &&
                     request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
                blogId = request.QueryStringParameters[ID_QUERY_STRING_NAME];

            if (string.IsNullOrEmpty(blogId))
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
                };

            context.Logger.LogLine($"Getting blog {blogId}");
            Blog blog = await this.DDBContext.LoadAsync<Blog>(blogId);
            context.Logger.LogLine($"Found blog: {blog != null}");

            if (blog == null)
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.NotFound
                };

            APIGatewayProxyResponse response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(blog),
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };
            return response;
        }

        /// <summary>
        ///     A Lambda function that adds a blog post.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> AddBlogAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            Blog blog = JsonConvert.DeserializeObject<Blog>(request?.Body);
            blog.Id = Guid.NewGuid().ToString();
            blog.CreatedTimestamp = DateTime.Now;

            context.Logger.LogLine($"Saving blog with id {blog.Id}");
            await this.DDBContext.SaveAsync(blog);

            APIGatewayProxyResponse response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = blog.Id,
                Headers = new Dictionary<string, string> {{"Content-Type", "text/plain"}}
            };
            return response;
        }

        /// <summary>
        ///     A Lambda function that removes a blog post from the DynamoDB table.
        /// </summary>
        /// <param name="request"></param>
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> RemoveBlogAsync(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            string blogId = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
                blogId = request.PathParameters[ID_QUERY_STRING_NAME];
            else if (request.QueryStringParameters != null &&
                     request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
                blogId = request.QueryStringParameters[ID_QUERY_STRING_NAME];

            if (string.IsNullOrEmpty(blogId))
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
                };

            context.Logger.LogLine($"Deleting blog with id {blogId}");
            await this.DDBContext.DeleteAsync<Blog>(blogId);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK
            };
        }
    }
}