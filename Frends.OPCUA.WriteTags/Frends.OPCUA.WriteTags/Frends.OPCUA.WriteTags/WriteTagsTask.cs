using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OPCUA.WriteTags;

public enum AuthenticationMethod
{
    Anonymous, UserIdentity
}

public class WriteValue
{
    public string NodeId { get; set; }
    public object Value { get; set; }
}

public class WriteValueResult
{
    public string NodeId { get; set; }
    public object Value { get; set; }
    public string ResultCode { get; set; }
}

public class WriteResult
{
    public bool IsAllSuccess { get; set; }
    public WriteValueResult[] Results { get; set; } = Array.Empty<WriteValueResult>();
    public string Reason { get; set; } = String.Empty;
}

public class Input
{
    /// <summary>
    /// The URL with protocol and path.
    /// </summary>
    [DefaultValue("opc.tcp://localhost:62541/Quickstarts/ReferenceServer")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Url { get; set; }
    
    /// <summary>
    /// Array of values to be written.
    /// </summary>
    public WriteValue[] WriteValues { get; set; }
}

public class Options
{
    /// <summary>
    /// Name of application client
    /// </summary>
    public string ApplicationName { get; set; }
    
    /// <summary>
    /// Set if all server certificates should be trusted
    /// </summary>
    public bool TrustServerCertificate { get; set; }

    /// <summary>
    /// Set thumbprint of trusted certificate
    /// </summary>
    public string TrustedCertificateThumbprint { get; set; }
    
    /// <summary>
    /// Method of client authentication
    /// </summary>
    public AuthenticationMethod Authentication { get; set; }

    /// <summary>
    /// If UserIdentity is selected you should fill Username and Password fields
    /// </summary>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.UserIdentity)]
    public string Username { get; set; }

    [PasswordPropertyText]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.UserIdentity)]
    public string Password { get; set; }
}

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