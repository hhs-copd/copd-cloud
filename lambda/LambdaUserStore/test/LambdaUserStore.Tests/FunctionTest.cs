using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using Xunit;

namespace LambdaUserStore.Tests
{
    public sealed class FunctionTest : IDisposable
    {
        public FunctionTest()
        {
            this.TableName = "BlueprintBaseName-Blogs-" + DateTime.Now.Ticks;
            this.DDBClient = new AmazonDynamoDBClient(RegionEndpoint.EUCentral1);

            this.SetupTableAsync().Wait();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private string TableName { get; }
        private IAmazonDynamoDB DDBClient { get; }


        /// <summary>
        ///     Create the DynamoDB table for testing. This table is deleted as part of the object dispose method.
        /// </summary>
        /// <returns></returns>
        private async Task SetupTableAsync()
        {
            CreateTableRequest request = new CreateTableRequest
            {
                TableName = this.TableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 2,
                    WriteCapacityUnits = 2
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        KeyType = KeyType.HASH,
                        AttributeName = Functions.ID_QUERY_STRING_NAME
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = Functions.ID_QUERY_STRING_NAME,
                        AttributeType = ScalarAttributeType.S
                    }
                }
            };

            await this.DDBClient.CreateTableAsync(request);

            DescribeTableRequest describeRequest = new DescribeTableRequest {TableName = this.TableName};
            DescribeTableResponse response;
            do
            {
                Thread.Sleep(1000);
                response = await this.DDBClient.DescribeTableAsync(describeRequest);
            } while (response.Table.TableStatus != TableStatus.ACTIVE);
        }

        private bool disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (this.disposedValue) return;

            if (disposing)
            {
                this.DDBClient.DeleteTableAsync(this.TableName).Wait();
                this.DDBClient.Dispose();
            }

            this.disposedValue = true;
        }

        [Fact]
        public async Task BlogTestAsync()
        {
            Functions functions = new Functions(this.DDBClient, this.TableName);

            // Add a new blog post
            Blog myBlog = new Blog
            {
                Name = "The awesome post",
                Content = "Content for the awesome blog"
            };

            APIGatewayProxyRequest request = new APIGatewayProxyRequest
            {
                Body = JsonConvert.SerializeObject(myBlog)
            };
            TestLambdaContext context = new TestLambdaContext();
            APIGatewayProxyResponse response = await functions.AddBlogAsync(request, context);
            Assert.Equal(200, response.StatusCode);

            string blogId = response.Body;

            // Confirm we can get the blog post back out
            request = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string> {{Functions.ID_QUERY_STRING_NAME, blogId}}
            };
            context = new TestLambdaContext();
            response = await functions.GetBlogAsync(request, context);
            Assert.Equal(200, response.StatusCode);

            Blog readBlog = JsonConvert.DeserializeObject<Blog>(response.Body);
            Assert.Equal(myBlog.Name, readBlog.Name);
            Assert.Equal(myBlog.Content, readBlog.Content);

            // List the blog posts
            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            response = await functions.GetBlogsAsync(request, context);
            Assert.Equal(200, response.StatusCode);

            Blog[] blogPosts = JsonConvert.DeserializeObject<Blog[]>(response.Body);
            Assert.Single(blogPosts);
            Assert.Equal(myBlog.Name, blogPosts[0].Name);
            Assert.Equal(myBlog.Content, blogPosts[0].Content);


            // Delete the blog post
            request = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string> {{Functions.ID_QUERY_STRING_NAME, blogId}}
            };
            context = new TestLambdaContext();
            response = await functions.RemoveBlogAsync(request, context);
            Assert.Equal(200, response.StatusCode);

            // Make sure the post was deleted.
            request = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string> {{Functions.ID_QUERY_STRING_NAME, blogId}}
            };
            context = new TestLambdaContext();
            response = await functions.GetBlogAsync(request, context);
            Assert.Equal((int) HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}