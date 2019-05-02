using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.Util;
using Newtonsoft.Json;
using Xunit;

namespace LambdaUserStore.Tests
{
    public sealed class FunctionTest : IDisposable
    {
        public void Dispose()
        {
            this.Dispose(true);
        }

        private string TableName { get; } = "BlueprintBaseName-DataEntries-" + DateTime.Now.Ticks;
        private IAmazonDynamoDB DDBClient { get; } = new AmazonDynamoDBClient(RegionEndpoint.EUCentral1);


        /// <summary>
        ///     Create the DynamoDB table for testing. This table is deleted as part of the object dispose method.
        /// </summary>
        /// <returns></returns>
        private async Task SetupTableAsync()
        {
            var request = new CreateTableRequest
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
                        AttributeName = Functions.UserIdQueryStringName
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = Functions.UserIdQueryStringName,
                        AttributeType = ScalarAttributeType.S
                    }
                }
            };

            await this.DDBClient.CreateTableAsync(request, CancellationToken.None);

            DescribeTableResponse response;
            do
            {
                await Task.Delay(1000);
                var tableRequest = new DescribeTableRequest {TableName = this.TableName};
                response = await this.DDBClient.DescribeTableAsync(tableRequest);
            } while (response.Table.TableStatus != TableStatus.ACTIVE);
        }

        private bool _disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (this._disposedValue) return;

            if (disposing)
            {
                this.DDBClient.DeleteTableAsync(this.TableName).Wait();
                this.DDBClient.Dispose();
            }

            this._disposedValue = true;
        }

        [Fact]
        public async Task DataEntryTestAsync()
        {
            await this.SetupTableAsync();

            var functions = new Functions(this.DDBClient, this.TableName);

            // Add a new data entry
            var now = DateTimeOffset.Now;
            var then = DateTimeOffset.Now - TimeSpan.FromSeconds(50);
            IEnumerable<DataEntry> myDataEntry = new List<DataEntry>
            {
                new DataEntry
                {
                    Humidity = 1.233f,
                    TimeStamp = now
                },
                new DataEntry
                {
                    GyroX = 12,
                    GyroY = -250,
                    GyroZ = 550,
                    TimeStamp = then
                }
            };

            var request = new APIGatewayProxyRequest
            {
                Headers = new Dictionary<string, string> {{ HeaderKeys.AuthorizationHeader, "userId" }},
                Body = JsonConvert.SerializeObject(myDataEntry)
            };
            var context = new TestLambdaContext();
            var response = await functions.AddDataAsync(request, context);
            Assert.Equal(201, response.StatusCode);

            // Confirm we can get the data entries back out
            request = new APIGatewayProxyRequest
            {
                Headers = new Dictionary<string, string> {{ HeaderKeys.AuthorizationHeader, "userId" }},
                PathParameters = new Dictionary<string, string> {{Functions.UserIdQueryStringName, "userId"}}
            };
            context = new TestLambdaContext();
            response = await functions.GetDataEntriesAsync(request, context);
            Assert.Equal(200, response.StatusCode);

            IEnumerable<DataEntry> readDataEntries = JsonConvert.DeserializeObject<IEnumerable<DataEntry>>(response.Body);
            Assert.Equal(readDataEntries.First().UserId, "userId");
            Assert.Equal(readDataEntries.First().Humidity, 1.233f);
            Assert.Equal(readDataEntries.First().TimeStamp, now);
            Assert.Equal(readDataEntries.Last().UserId, "userId");
            Assert.Equal(readDataEntries.Last().GyroX, 12);
            Assert.Equal(readDataEntries.Last().GyroY, -250);
            Assert.Equal(readDataEntries.Last().GyroZ, 550);
            Assert.Equal(readDataEntries.Last().TimeStamp, then);
        }
    }
}