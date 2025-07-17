using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.WebhookTester.Tests;

public class WebhookTesterPublicApiTests
{
    [Fact]
    public void AddWebhookTesterShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string name = "webhook";

        var action = () => builder.AddWebhookTester(name);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void AddWebhookTesterShouldThrowWhenNameIsNull()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        string name = null!;

        var action = () => builder.AddWebhookTester(name);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(name), exception.ParamName);
    }

    [Fact]
    public void WithFsStorageDirShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<WebhookTesterResource> builder = null!;

        var action = () => builder.WithFsStorageDir("/tmp");

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithFsStorageDirShouldThrowWhenDirIsNull()
    {
        var builderResource = DistributedApplication.CreateBuilder([]);
        var webhook = builderResource.AddWebhookTester("webhook");
        string dir = null!;

        var action = () => webhook.WithFsStorageDir(dir);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(dir), exception.ParamName);
    }

    [Fact]
    public void WithRedisDsnShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<WebhookTesterResource> builder = null!;

        var action = () => builder.WithRedisDsn("redis://localhost:6379");

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithRedisDsnShouldThrowWhenDsnIsNull()
    {
        var builderResource = DistributedApplication.CreateBuilder([]);
        var webhook = builderResource.AddWebhookTester("webhook");
        string dsn = null!;

        var action = () => webhook.WithRedisDsn(dsn);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(dsn), exception.ParamName);
    }

    [Fact]
    public void WithDefaultWebhookTokenShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<ContainerResource> builder = null!;
        var app = DistributedApplication.CreateBuilder([]);
        var webhook = app.AddWebhookTester("webhook");

        var action = () => builder.WithDefaultWebhookToken(webhook);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithDefaultWebhookTokenShouldThrowWhenWebhookIsNull()
    {
        var app = DistributedApplication.CreateBuilder([]);
        var container = app.AddContainer("app", "image");
        IResourceBuilder<WebhookTesterResource> webhook = null!;

        var action = () => container.WithDefaultWebhookToken(webhook);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(webhook), exception.ParamName);
    }

    [Fact]
    public void CtorWebhookTesterResourceShouldThrowWhenNameIsNull()
    {
        string name = null!;

        var action = () => new WebhookTesterResource(name, "token");

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(nameof(name), exception.ParamName);
    }

    [Fact]
    public void CtorWebhookTesterResourceShouldThrowWhenDefaultSessionTokenIsNull()
    {
        string token = null!;

        var action = () => new WebhookTesterResource("webhook", token);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("defaultSessionToken", exception.ParamName);
    }
}

