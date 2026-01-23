using ThunderPipe.Clients;
using ThunderPipe.Tests.Helpers;
using ThunderPipe.Tests.MockedObjects;
using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Clients;

public class ValidationApiClientTests
{
	[Fact]
	public async Task IsIconValid_WhenDataErrorReceived_ReturnError()
	{
		const string PATH = "~/icon.png";
		const string ERROR = "Expected in base64";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new Models.API.ValidateIcon.Response { DataErrors = [ERROR] });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsIconValid(PATH, fileSystem);

		Assert.Single(errors);
		Assert.True(errors.Contains(ERROR));
	}

	[Fact]
	public async Task IsIconValid_WhenValidationErrorReceived_ReturnError()
	{
		const string PATH = "~/icon.png";
		const string ERROR = "Icon must be 256x256";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new Models.API.ValidateIcon.Response { ValidationErrors = [ERROR] });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsIconValid(PATH, fileSystem);

		Assert.Single(errors);
		Assert.True(errors.Contains(ERROR));
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
				new Models.API.ValidateIcon.Response
				{
					DataErrors = [ERROR_1, ERROR_2],
					ValidationErrors = [ERROR_3, ERROR_4],
				}
			);

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsIconValid(PATH, fileSystem);

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4];

		Assert.Equal(expectedErrors.Length, errors.Count);

		foreach (var error in expectedErrors)
			Assert.True(errors.Contains(error));
	}

	[Fact]
	public async Task IsIconValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new Models.API.ValidateIcon.Response { Valid = false });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		try
		{
			_ = await client.IsIconValid(PATH, fileSystem);
		}
		catch (Exception e)
		{
			Assert.IsType<InvalidOperationException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}

	[Fact]
	public async Task IsIconValid_WhenErrorReceivedButMarkedAsValid_ThrowException()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(
				new Models.API.ValidateIcon.Response { DataErrors = ["Error"], Valid = true }
			);

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		try
		{
			_ = await client.IsIconValid(PATH, fileSystem);
		}
		catch (Exception e)
		{
			Assert.IsType<InvalidOperationException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}

	[Fact]
	public async Task IsIconValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new Models.API.ValidateIcon.Response { Valid = true });

		fileSystem.SetContent(PATH, await ImageHelper.CreateImage(1, 1));

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsIconValid(PATH, fileSystem);

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

		mockedHttp
			.When("*")
			.RespondJSON(new Models.API.ValidateManifest.Response { DataErrors = [ERROR] });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem);

		Assert.Single(errors);
		Assert.True(errors.Contains(ERROR));
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
			.RespondJSON(new Models.API.ValidateManifest.Response { ValidationErrors = [ERROR] });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem);

		Assert.Single(errors);
		Assert.True(errors.Contains(ERROR));
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
			.RespondJSON(new Models.API.ValidateManifest.Response { NamespaceErrors = [ERROR] });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem);

		Assert.Single(errors);
		Assert.True(errors.Contains(ERROR));
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
				new Models.API.ValidateManifest.Response
				{
					DataErrors = [ERROR_1, ERROR_2],
					ValidationErrors = [ERROR_3, ERROR_4],
					NamespaceErrors = [ERROR_5, ERROR_6],
				}
			);

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem);

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4, ERROR_5, ERROR_6];

		Assert.Equal(expectedErrors.Length, errors.Count);

		foreach (var error in expectedErrors)
			Assert.True(errors.Contains(error));
	}

	[Fact]
	public async Task IsManifestValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new Models.API.ValidateManifest.Response { Valid = false });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		try
		{
			_ = await client.IsManifestValid(PATH, TEAM, fileSystem);
		}
		catch (Exception e)
		{
			Assert.IsType<InvalidOperationException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
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
			.RespondJSON(
				new Models.API.ValidateManifest.Response { DataErrors = ["Error"], Valid = true }
			);

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		try
		{
			_ = await client.IsManifestValid(PATH, TEAM, fileSystem);
		}
		catch (Exception e)
		{
			Assert.IsType<InvalidOperationException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}

	[Fact]
	public async Task IsManifestValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new Models.API.ValidateManifest.Response { Valid = true });

		fileSystem.SetContent(PATH, "{}");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsManifestValid(PATH, TEAM, fileSystem);

		Assert.Empty(errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenDataErrorReceived_ReturnError()
	{
		const string PATH = "~/README.md";
		const string ERROR = "Expected in base64";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(new Models.API.ValidateReadme.Response { DataErrors = [ERROR] });

		fileSystem.SetContent(PATH, "this is a great README");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsReadmeValid(PATH, fileSystem);

		Assert.Single(errors);
		Assert.True(errors.Contains(ERROR));
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
			.RespondJSON(new Models.API.ValidateReadme.Response { ValidationErrors = [ERROR] });

		fileSystem.SetContent(PATH, "aww :(");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsReadmeValid(PATH, fileSystem);

		Assert.Single(errors);
		Assert.True(errors.Contains(ERROR));
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
				new Models.API.ValidateReadme.Response
				{
					DataErrors = [ERROR_1, ERROR_2],
					ValidationErrors = [ERROR_3, ERROR_4],
				}
			);

		fileSystem.SetContent(PATH, "yay :D");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsReadmeValid(PATH, fileSystem);

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4];

		Assert.Equal(expectedErrors.Length, errors.Count);

		foreach (var error in expectedErrors)
			Assert.True(errors.Contains(error));
	}

	[Fact]
	public async Task IsReadmeValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/README.md";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new Models.API.ValidateReadme.Response { Valid = false });

		fileSystem.SetContent(PATH, "oh?");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		try
		{
			_ = await client.IsReadmeValid(PATH, fileSystem);
		}
		catch (Exception e)
		{
			Assert.IsType<InvalidOperationException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}

	[Fact]
	public async Task IsReadmeValid_WhenErrorReceivedButMarkedAsValid_ThrowException()
	{
		const string PATH = "~/README.md";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp
			.When("*")
			.RespondJSON(
				new Models.API.ValidateReadme.Response { DataErrors = ["Error"], Valid = true }
			);

		fileSystem.SetContent(PATH, "oh?");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		try
		{
			_ = await client.IsReadmeValid(PATH, fileSystem);
		}
		catch (Exception e)
		{
			Assert.IsType<InvalidOperationException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}

	[Fact]
	public async Task IsReadmeValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/README.md";

		var mockedHttp = new MockHttpMessageHandler();
		var fileSystem = new TestFileSystem();

		mockedHttp.When("*").RespondJSON(new Models.API.ValidateReadme.Response { Valid = true });

		fileSystem.SetContent(PATH, "oh?");

		using var client = new ValidationApiClient(
			new RequestBuilder(),
			mockedHttp.ToHttpClient(),
			CancellationToken.None
		);

		var errors = await client.IsReadmeValid(PATH, fileSystem);

		Assert.Empty(errors);
	}
}
