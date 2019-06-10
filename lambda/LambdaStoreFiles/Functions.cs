using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using LambdaUserStore;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace LambdaStoreFiles
{
    public class Functions
    {
        private const string bucketName = "copd-storage";
        private static readonly RegionEndpoint regionEndpoint = RegionEndpoint.EUCentral1;
        private static readonly IAmazonS3 s3Client = new AmazonS3Client(regionEndpoint);

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public static async Task<APIGatewayProxyResponse> PostData(APIGatewayProxyRequest request, ILambdaContext context)
        {
            FileContent file = JsonConvert.DeserializeObject<FileContent>(request.Body);

            if (string.IsNullOrEmpty(file.FileName) || string.IsNullOrEmpty(file.Content))
            {
                return RequestUtility.ErrorResponse(HttpStatusCode.BadRequest, "Request doesnt contain filename or content in JSON");
            }

            try
            {
                using (AmazonS3Client client = new AmazonS3Client(regionEndpoint))
                {
                    PutObjectRequest putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = file.FileName,
                        ContentBody = file.Content
                    };
                    PutObjectResponse response = await client.PutObjectAsync(putRequest);

                    return RequestUtility.ErrorResponse(response.HttpStatusCode, "called service");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in PutS3Object:" + ex.Message);
                return RequestUtility.ErrorResponse(HttpStatusCode.InternalServerError, "Something went wrong while trying to fetch the data");
            }

        }
    }
}