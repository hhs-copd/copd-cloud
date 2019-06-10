using System;
using Amazon.DynamoDBv2.Model;

namespace LambdaStoreFiles.CSV
{
    public interface IItem
    {
        string Name { get; }
        
        DateTimeOffset DateTime { get; }
        
        AttributeValue Value { get; }
    }
}
