using System.Runtime.CompilerServices;
using Opc.Ua;
using Opc.Ua.Client;

[assembly: InternalsVisibleTo("Frends.OPCUA.WriteTags.Tests")]
namespace Frends.OPCUA.WriteTags;

public interface IOpcUaClientFactory
{
    IOpcUaClient CreateClient(Input inputData, Options clientOptions);
}

internal class OpcUaClientFactory : IOpcUaClientFactory
{
    public IOpcUaClient CreateClient(Input inputData, Options clientOptions)
    {
        var appConfig = new ApplicationConfiguration
        {
            ApplicationType = ApplicationType.Client,
            ApplicationName = clientOptions.ApplicationName,
            SecurityConfiguration = new SecurityConfiguration(),
            CertificateValidator = new CertificateValidator(),
            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = 60000,
                WellKnownDiscoveryUrls = new StringCollection
                {
                    "opc.tcp://{0}:4840",
                    "http://{0}:52601/UADiscovery",
                    "http://{0}/UADiscovery/Default.svc"
                },
                DiscoveryServers = null,
                MinSubscriptionLifetime = 10000,
                OperationLimits = new OperationLimits
                {
                    MaxNodesPerRead = 2500,
                    MaxNodesPerHistoryReadData = 1000,
                    MaxNodesPerHistoryReadEvents = 1000,
                    MaxNodesPerWrite = 2500,
                    MaxNodesPerHistoryUpdateData = 1000,
                    MaxNodesPerHistoryUpdateEvents = 1000,
                    MaxNodesPerMethodCall = 2500,
                    MaxNodesPerBrowse = 2500,
                    MaxNodesPerRegisterNodes = 2500,
                    MaxNodesPerTranslateBrowsePathsToNodeIds = 2500,
                    MaxNodesPerNodeManagement = 2500,
                    MaxMonitoredItemsPerCall = 2500,
                }
            }
        };

        appConfig.CertificateValidator.CertificateValidation += (sender, eventArgs) =>
        {
            eventArgs.Accept = ShouldAcceptCertificate(clientOptions, eventArgs.Certificate.Thumbprint);
        };

        var client = new OpcUaClient(appConfig, inputData.Url);

        if (clientOptions.Authentication == AuthenticationMethod.UserIdentity)
        {
            client.UserIdentity = new UserIdentity(clientOptions.Username, clientOptions.Password);
        }

        return client;
    }

    public bool ShouldAcceptCertificate(Options options, string certThumbprint)
    {
        if (options.TrustServerCertificate)
        {
            return true;
        }

        return certThumbprint.Equals(options.TrustedCertificateThumbprint,
            StringComparison.InvariantCultureIgnoreCase);
    }
}

public interface IOpcUaClient
{
    Task<WriteResult> WriteDataAsync(WriteValue[] writeValues, CancellationToken ct = default);
}

public class OpcSessionException : Exception
{
    public OpcSessionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

internal class OpcUaClient : IOpcUaClient
{
    public ApplicationConfiguration ClientConfig { get; }
    public string ServerUrl { get; }
    private const uint SessionLifeTime = 60000;
    public IUserIdentity? UserIdentity { get; set; } = null;

    public OpcUaClient(ApplicationConfiguration clientConfig, string serverUrl)
    {
        ClientConfig = clientConfig ?? throw new ArgumentException("Client config must be specified", nameof(clientConfig));
        ServerUrl = string.IsNullOrEmpty(serverUrl) ? throw new ArgumentException("Server URL must be specified", nameof(serverUrl)) : serverUrl;
    }

    public async Task<WriteResult> WriteDataAsync(WriteValue[] writeValues, CancellationToken ct = default)
    {
        ThrowIfWriteValuesNotValid(writeValues);

        var result = new WriteResult();
        var session = await CreateSessionAsync(ct).ConfigureAwait(false);
        
        ct.ThrowIfCancellationRequested();

        try
        {
            var toBeWritten = writeValues.Select(value => new Opc.Ua.WriteValue
            {
                NodeId = new NodeId(value.NodeId),
                AttributeId = Attributes.Value,
                Value = new DataValue
                {
                    Value = value.Value
                }
            });

            var response = await session.WriteAsync(null, new WriteValueCollection(toBeWritten), ct);

            //map to writeResult
            result.Results = writeValues.Select((valueResult, i) => new WriteValueResult
            {
                NodeId = valueResult.NodeId,
                Value = valueResult.Value,
                ResultCode = response.Results[i].ToString()
            }).ToArray();
            result.IsAllSuccess = result.Results.All(r => r.ResultCode.Equals("Good", StringComparison.InvariantCultureIgnoreCase));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            session?.CloseAsync(true, ct).ConfigureAwait(false);
            session?.Dispose();
        }

        return result;
    }

    private void ThrowIfWriteValuesNotValid(WriteValue[] writeValues)
    {
        if (writeValues == null)
        {
            throw new ArgumentException("Values to be written is null", nameof(writeValues));
        }

        if (!writeValues.Any())
        {
            throw new ArgumentException("Values to be written are empty", nameof(writeValues));
        }
    }

    private async Task<ISession> CreateSessionAsync(CancellationToken ct = default)
    {
        try
        {
            var f = DefaultSessionFactory.Instance;

            ITransportWaitingConnection? connection = null;
            var endpointDescription = CoreClientUtils.SelectEndpoint(ClientConfig, ServerUrl, false);

            var endpointConfiguration = EndpointConfiguration.Create(ClientConfig);
            var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            var session =
                await f.CreateAsync(
                    ClientConfig,
                    connection,
                    endpoint,
                    connection == null,
                    false,
                    ClientConfig.ApplicationName,
                    SessionLifeTime,
                    UserIdentity,
                    null,
                    ct
                ).ConfigureAwait(false);
            return session;
        }
        catch (Exception e)
        {
            throw new OpcSessionException("Could not create session", e);
        }
    }
}