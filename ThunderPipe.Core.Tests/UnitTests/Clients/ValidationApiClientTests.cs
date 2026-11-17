using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Tests.Infrastructure;
using ThunderPipe.Core.Tests.Infrastructure.Fixtures;
using ThunderPipe.Core.Tests.MockedObjects;
using ValidateIcon = ThunderPipe.Core.DTOs.ValidateIcon;
using ValidateManifest = ThunderPipe.Core.DTOs.ValidateManifest;
using ValidateReadme = ThunderPipe.Core.DTOs.ValidateReadme;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_CLIENT)]
public class ValidationApiClientTests : IClassFixture<HttpFixture>
{
	private readonly HttpFixture _fixture;
	private readonly IFileSystem _fileSystem;

	public ValidationApiClientTests(HttpFixture fixture)
	{
		_fixture = fixture;
		_fileSystem = new TestFileSystem();
	}

	[Fact]
	public async Task IsIconValid_WhenDataErrorReceived_ReturnError()
	{
		const string PATH = "~/icon.png";
		const string CATEGORY = "icon_data";
		const string ERROR = "Expected in base64";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(new Dictionary<string, string[]> { [CATEGORY] = [ERROR] });

		await _fileSystem.WriteAllTextAsync(
			PATH,
			string.Empty,
			TestContext.Current.CancellationToken
		);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsIconValid(
			PATH,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsIconValid_WhenValidationErrorReceived_ReturnError()
	{
		const string PATH = "~/icon.png";
		const string CATEGORY = "non_field_errors";
		const string ERROR = "Icon must be 256x256";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(new Dictionary<string, string[]> { [CATEGORY] = [ERROR] });

		await _fileSystem.WriteAllTextAsync(
			PATH,
			string.Empty,
			TestContext.Current.CancellationToken
		);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsIconValid(
			PATH,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsIconValid_WhenMultipleErrorsReceived_ReturnErrors()
	{
		const string PATH = "~/icon.png";
		const string CATEGORY_1 = "icon_data";
		const string CATEGORY_2 = "non_field_errors";
		const string ERROR_1 = "This is an error";
		const string ERROR_2 = "This could be an error";
		const string ERROR_3 = "This should be an error";
		const string ERROR_4 = "This might be an error";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(
				new Dictionary<string, string[]>
				{
					[CATEGORY_1] = [ERROR_1, ERROR_2],
					[CATEGORY_2] = [ERROR_3, ERROR_4],
				}
			);

		await _fileSystem.WriteAllTextAsync(
			PATH,
			string.Empty,
			TestContext.Current.CancellationToken
		);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsIconValid(
			PATH,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4];

		Assert.Missing(expectedErrors, errors);
	}

	[Fact]
	public async Task IsIconValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp.When("*").RespondJSON(new ValidateIcon.Response { Valid = false });

		await _fileSystem.WriteAllTextAsync(
			PATH,
			string.Empty,
			TestContext.Current.CancellationToken
		);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsIconValid(PATH, _fileSystem, "", TestContext.Current.CancellationToken)
		);
	}

	[Fact]
	public async Task IsIconValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/icon.png";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp.When("*").RespondJSON(new ValidateIcon.Response { Valid = true });

		await _fileSystem.WriteAllTextAsync(
			PATH,
			string.Empty,
			TestContext.Current.CancellationToken
		);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsIconValid(
			PATH,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Empty(errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenDataErrorReceived_ReturnError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";
		const string CATEGORY = "manifest_data";
		const string ERROR = "Expected in base64";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(new Dictionary<string, string[]> { [CATEGORY] = [ERROR] });

		await _fileSystem.WriteAllTextAsync(PATH, "{}", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsManifestValid(
			PATH,
			TEAM,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenValidationErrorReceived_ReturnError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";
		const string CATEGORY = "non_field_errors";
		const string ERROR = "Expected in base64";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(new Dictionary<string, string[]> { [CATEGORY] = [ERROR] });

		await _fileSystem.WriteAllTextAsync(PATH, "{}", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsManifestValid(
			PATH,
			TEAM,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenNamespaceErrorReceived_ReturnError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";
		const string CATEGORY = "namespace";
		const string ERROR = "Expected in base64";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(new Dictionary<string, string[]> { [CATEGORY] = [ERROR] });

		await _fileSystem.WriteAllTextAsync(PATH, "{}", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsManifestValid(
			PATH,
			TEAM,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenMultipleErrorsReceived_ReturnErrors()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";
		const string CATEGORY_1 = "manifest_data";
		const string CATEGORY_2 = "non_field_errors";
		const string CATEGORY_3 = "namespace";
		const string ERROR_1 = "This is an error";
		const string ERROR_2 = "This could be an error";
		const string ERROR_3 = "This should be an error";
		const string ERROR_4 = "This might be an error";
		const string ERROR_5 = "This would be an error";
		const string ERROR_6 = "This was be an error";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(
				new Dictionary<string, string[]>
				{
					[CATEGORY_1] = [ERROR_1, ERROR_2],
					[CATEGORY_2] = [ERROR_3, ERROR_4],
					[CATEGORY_3] = [ERROR_5, ERROR_6],
				}
			);

		await _fileSystem.WriteAllTextAsync(PATH, "{}", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsManifestValid(
			PATH,
			TEAM,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4, ERROR_5, ERROR_6];

		Assert.Missing(expectedErrors, errors);
	}

	[Fact]
	public async Task IsManifestValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp.When("*").RespondJSON(new ValidateManifest.Response { Valid = false });

		await _fileSystem.WriteAllTextAsync(PATH, "{}", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsManifestValid(
				PATH,
				TEAM,
				_fileSystem,
				"",
				TestContext.Current.CancellationToken
			)
		);
	}

	[Fact]
	public async Task IsManifestValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/manifest.json";
		const string TEAM = "test-team";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp.When("*").RespondJSON(new ValidateManifest.Response { Valid = true });

		await _fileSystem.WriteAllTextAsync(PATH, "{}", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsManifestValid(
			PATH,
			TEAM,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Empty(errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenDataErrorReceived_ReturnError()
	{
		const string PATH = "~/README.md";
		const string CATEGORY = "readme_data";
		const string ERROR = "Expected in base64";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(new Dictionary<string, string[]>() { [CATEGORY] = [ERROR] });

		await _fileSystem.WriteAllTextAsync(
			PATH,
			"this is a great README",
			TestContext.Current.CancellationToken
		);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsReadmeValid(
			PATH,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenValidationErrorReceived_ReturnError()
	{
		const string PATH = "~/README.md";
		const string CATEGORY = "non_field_errors";
		const string ERROR = "too fat";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(new Dictionary<string, string[]>() { [CATEGORY] = [ERROR] });

		await _fileSystem.WriteAllTextAsync(PATH, "aww :(", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsReadmeValid(
			PATH,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Single(errors);
		Assert.Contains(ERROR, errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenMultipleErrorsReceived_ReturnErrors()
	{
		const string PATH = "~/README.md";
		const string CATEGORY_1 = "readme_data";
		const string CATEGORY_2 = "non_field_errors";
		const string ERROR_1 = "This is an error";
		const string ERROR_2 = "This could be an error";
		const string ERROR_3 = "This should be an error";
		const string ERROR_4 = "This might be an error";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp
			.When("*")
			.RespondErrors(
				new Dictionary<string, string[]>()
				{
					[CATEGORY_1] = [ERROR_1, ERROR_2],
					[CATEGORY_2] = [ERROR_3, ERROR_4],
				}
			);

		await _fileSystem.WriteAllTextAsync(PATH, "yay :D", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsReadmeValid(
			PATH,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		string[] expectedErrors = [ERROR_1, ERROR_2, ERROR_3, ERROR_4];

		Assert.Missing(expectedErrors, errors);
	}

	[Fact]
	public async Task IsReadmeValid_WhenNoErrorButNotMarkedAsValid_ThrowException()
	{
		const string PATH = "~/README.md";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp.When("*").RespondJSON(new ValidateReadme.Response { Valid = false });

		await _fileSystem.WriteAllTextAsync(PATH, "oh?", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await client.IsReadmeValid(PATH, _fileSystem, "", TestContext.Current.CancellationToken)
		);
	}

	[Fact]
	public async Task IsReadmeValid_WhenNoErrorAndMarkedAsValid_ReturnNoError()
	{
		const string PATH = "~/README.md";

		var mockedHttp = _fixture.MockHttp();

		mockedHttp.When("*").RespondJSON(new ValidateReadme.Response { Valid = true });

		await _fileSystem.WriteAllTextAsync(PATH, "oh?", TestContext.Current.CancellationToken);

		var client = new ValidationApiClient(mockedHttp.ToHttpClient(), _fixture.Builder());

		var errors = await client.IsReadmeValid(
			PATH,
			_fileSystem,
			"",
			TestContext.Current.CancellationToken
		);

		Assert.Empty(errors);
	}
}
