using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Tests.Helpers;
using ThunderPipe.Core.Tests.MockedObjects;
using ValidateIcon = ThunderPipe.Core.Models.Web.ValidateIcon;
using ValidateManifest = ThunderPipe.Core.Models.Web.ValidateManifest;
using ValidateReadme = ThunderPipe.Core.Models.Web.ValidateReadme;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

public class ValidationApiClientTests
{
	[Fact]
	public async Task IsIconValid_WhenDataErrorReceived_ReturnError()
	{
		const string PATH = "~/icon.png";
		const string ERROR = "Expected in base64";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateIcon.Response { DataErrors = [ERROR] });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsIconValid(PATH, fileSystem, "");

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsIconValid_WhenValidationErrorReceived_ReturnError()
	{
		const string PATH = "~/icon.png";
		const string ERROR = "Icon must be 256x256";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateIcon.Response { ValidationErrors = [ERROR] });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsIconValid(PATH, fileSystem, "");

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsIconValid_WhenMultipleErrorsReceived_ReturnErrors()
	{
		const string PATH = "~/icon.png";
		const string ERROR_1 = "This is an error";
		const string ERROR_2 = "This could be an error";
		const string ERROR_3 = "This should be an error";
		const string ERROR_4 = "This might be an error";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(
				new ValidateIcon.Response
				{
					DataErrors = [ERROR_1, ERROR_2],
					ValidationErrors = [ERROR_3, ERROR_4],
				}
			);

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsIconValid(PATH, fileSystem, "");

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4];

		Assert.Equal(expectedErrors.Length, errors.Count);

		foreach (var error in expectedErrors)
			Assert.Contains(error, errors);
	}

	[Fact]
	public async Task IsIconValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateIcon.Response { Valid = false });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsIconValid(PATH, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsIconValid_WhenNoErrorButMissingValid_ThrowException()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateIcon.Response());

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsIconValid(PATH, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsIconValid_WhenErrorReceivedButMarkedAsValid_ThrowException()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new ValidateIcon.Response { DataErrors = ["Error"], Valid = true });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsIconValid(PATH, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsIconValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateIcon.Response { Valid = true });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsIconValid(PATH, fileSystem, "");

		Assert.Empty(errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenDataErrorReceived_ReturnError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";
		const string ERROR = "Expected in base64";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateManifest.Response { DataErrors = [ERROR] });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem, "");

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenValidationErrorReceived_ReturnError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";
		const string ERROR = "Expected in base64";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new ValidateManifest.Response { ValidationErrors = [ERROR] });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem, "");

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenNamespaceErrorReceived_ReturnError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";
		const string ERROR = "Expected in base64";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new ValidateManifest.Response { NamespaceErrors = [ERROR] });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem, "");

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenMultipleErrorsReceived_ReturnErrors()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";
		const string ERROR_1 = "This is an error";
		const string ERROR_2 = "This could be an error";
		const string ERROR_3 = "This should be an error";
		const string ERROR_4 = "This might be an error";
		const string ERROR_5 = "This would be an error";
		const string ERROR_6 = "This was be an error";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(
				new ValidateManifest.Response
				{
					DataErrors = [ERROR_1, ERROR_2],
					ValidationErrors = [ERROR_3, ERROR_4],
					NamespaceErrors = [ERROR_5, ERROR_6],
				}
			);

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem, "");

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4, ERROR_5, ERROR_6];

		Assert.Equal(expectedErrors.Length, errors.Count);

		foreach (var error in expectedErrors)
			Assert.Contains(error, errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateManifest.Response { Valid = false });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsManifestValid(PATH, TEAM, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsManifestValid_WhenNoErrorButMissingValid_ThrowException()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateManifest.Response());

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsManifestValid(PATH, TEAM, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsManifestValid_WhenErrorReceivedButMarkedAsValid_ThrowException()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new ValidateManifest.Response { DataErrors = ["Error"], Valid = true });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsManifestValid(PATH, TEAM, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsManifestValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateManifest.Response { Valid = true });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem, "");

		Assert.Empty(errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenDataErrorReceived_ReturnError()
	{
		const string PATH = "~/README.md";
		const string ERROR = "Expected in base64";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateReadme.Response { DataErrors = [ERROR] });

		fileSystem.SetContent(PATH, "this is a great README");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsReadmeValid(PATH, fileSystem, "");

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenValidationErrorReceived_ReturnError()
	{
		const string PATH = "~/README.md";
		const string ERROR = "too fat";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new ValidateReadme.Response { ValidationErrors = [ERROR] });

		fileSystem.SetContent(PATH, "aww :(");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsReadmeValid(PATH, fileSystem, "");

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenMultipleErrorsReceived_ReturnErrors()
	{
		const string PATH = "~/README.md";
		const string ERROR_1 = "This is an error";
		const string ERROR_2 = "This could be an error";
		const string ERROR_3 = "This should be an error";
		const string ERROR_4 = "This might be an error";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(
				new ValidateReadme.Response
				{
					DataErrors = [ERROR_1, ERROR_2],
					ValidationErrors = [ERROR_3, ERROR_4],
				}
			);

		fileSystem.SetContent(PATH, "yay :D");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsReadmeValid(PATH, fileSystem, "");

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4];

		Assert.Equal(expectedErrors.Length, errors.Count);

		foreach (var error in expectedErrors)
			Assert.Contains(error, errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/README.md";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateReadme.Response { Valid = false });

		fileSystem.SetContent(PATH, "oh?");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsReadmeValid(PATH, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsReadmeValid_WhenNoErrorButMissingValid_ThrowException()
	{
		const string PATH = "~/README.md";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateReadme.Response());

		fileSystem.SetContent(PATH, "oh?");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsReadmeValid(PATH, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsReadmeValid_WhenErrorReceivedButMarkedAsValid_ThrowException()
	{
		const string PATH = "~/README.md";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new ValidateReadme.Response { DataErrors = ["Error"], Valid = true });

		fileSystem.SetContent(PATH, "oh?");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsReadmeValid(PATH, fileSystem, "")
		);
	}

	[Fact]
	public async Task IsReadmeValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/README.md";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new ValidateReadme.Response { Valid = true });

		fileSystem.SetContent(PATH, "oh?");

		using var client = new ValidationApiClient();
		client.Client = mockedHttp.ToHttpClient();

		var errors = await client.IsReadmeValid(PATH, fileSystem, "");

		Assert.Empty(errors);
	}
}
