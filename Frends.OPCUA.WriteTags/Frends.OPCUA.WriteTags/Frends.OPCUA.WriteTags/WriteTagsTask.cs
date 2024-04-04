using System.ComponentModel;
using Frends.OPCUA.WriteTags.Definitions;
using Frends.OPCUA.WriteTags.Services;

namespace Frends.OPCUA.WriteTags;
public static class OPCUA
{
    public static IOpcUaClientFactory ClientFactory { get; set; } = new OpcUaClientFactory();
    
    public static async Task<WriteResult> WriteTags([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        //validate all inputs and options
        var result = new WriteResult();
        var client = ClientFactory.CreateClient(input, options);

        try
        {
            result = await client.WriteDataAsync(input.WriteValues, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            result.IsAllSuccess = false;
            result.Reason = e.Message;
        }

        return result;
    }
}