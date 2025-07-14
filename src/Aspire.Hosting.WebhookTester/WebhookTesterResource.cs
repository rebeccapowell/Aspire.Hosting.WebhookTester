namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a webhook tester container resource with a session-based endpoint URL as a connection string.
/// </summary>
public sealed class WebhookTesterResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookTesterResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="defaultSessionToken">The token used for the default session URL.</param>
    public WebhookTesterResource(string name, string defaultSessionToken)
        : base(name)
    {
        DefaultSessionToken = defaultSessionToken;
    }

    /// <summary>
    /// The default session token used to construct the session URL.
    /// </summary>
    public string DefaultSessionToken { get; }

    /// <summary>
    /// The primary HTTP endpoint reference for this container.
    /// </summary>
    public EndpointReference PrimaryEndpoint =>
        _primaryEndpoint ??= new EndpointReference(this, PrimaryEndpointName);

    /// <summary>
    /// Gets the connection string expression for the Webhook Tester (e.g. "http://host:port/s/{token}").
    /// </summary>
    public ReferenceExpression ConnectionStringExpression => BuildConnectionStringExpression();

    /// <inheritdoc />
    public ReferenceExpression ConnectionString => ConnectionStringExpression;

    /// <inheritdoc />
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        if (this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var redirect))
        {
            return redirect.Resource.GetConnectionStringAsync(cancellationToken);
        }

        return ConnectionStringExpression.GetValueAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the ReferenceExpression for the webhook URL using the PrimaryEndpoint properties.
    /// </summary>
    private ReferenceExpression BuildConnectionStringExpression()
    {
        // Build the webhook tester session URL in one interpolated expression
        var builder = new ReferenceExpressionBuilder();
        builder.Append(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}/{DefaultSessionToken}");
        return builder.Build();
    }
}