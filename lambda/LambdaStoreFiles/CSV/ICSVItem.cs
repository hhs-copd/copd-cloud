using System;

namespace LambdaStoreFiles.CSV
{
    public interface ICSVItem
    {
        DateTimeOffset DateTime { get; }
    }
}
