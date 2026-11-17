using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThunderPipe.Core.Tests.Data;
using ThunderPipe.Core.Tests.Infrastructure;
using ThunderPipe.Core.Utils;

// ReSharper disable InconsistentNaming

namespace ThunderPipe.Core.Tests.UnitTests.Utils;

// TODO: Redo all tests to match proper syntax
// TODO: Get 100% coverage on RequestBuilder
[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_UTILS)]
public class RequestBuilderTests
{
	[Theory]
	[MemberData(
		nameof(RequestBuilderTestData.ValidMethods),
		MemberType = typeof(RequestBuilderTestData)
	)]
	public void SetMethod(HttpMethod method)
	{
		var builder = new RequestBuilder();

		switch (method.Method)
		{
			case "GET":
				builder.Get();
				break;
			case "POST":
				builder.Post();
				break;
			case "PUT":
				builder.Put();
				break;
			default:
				throw new InvalidOperationException($"Method '{method.Method}' is not valid.");
		}

		var request = builder.Build();
		Assert.Equal(method, request.Method);
	}

	[Theory]
	[MemberData(
		nameof(RequestBuilderTestData.ValidUris),
		MemberType = typeof(RequestBuilderTestData)
	)]
	public void ToUri_WhenSet_AssignUri(string url)
	{
		var uri = new Uri(url);
		var request = new RequestBuilder().ToUri(uri).Build();

		Assert.NotNull(request.RequestUri);
		Assert.Equal(uri.AbsoluteUri, request.RequestUri.AbsoluteUri);
	}

	[Theory]
	[MemberData(
		nameof(RequestBuilderTestData.ValidEndpoints),
		MemberType = typeof(RequestBuilderTestData)
	)]
	public void ToEndpoint_WhenSet_AssignEndpoint(string endpoint)
	{
		var uri = new Uri("https://www.google.com");
		var request = new RequestBuilder().ToUri(uri).ToEndpoint(endpoint).Build();

		Assert.NotNull(request.RequestUri);
		Assert.Equal(endpoint, request.RequestUri.AbsolutePath);
	}

	[Theory]
	[MemberData(
		nameof(RequestBuilderTestData.ValidTokens),
		MemberType = typeof(RequestBuilderTestData)
	)]
	public void WithAuth_WhenSet_AssignAuthToken(string token)
	{
		var request = new RequestBuilder().WithAuth(token).Build();

		Assert.NotNull(request.Headers.Authorization);
		Assert.Equal(token, request.Headers.Authorization.Parameter);
	}

	[Theory]
	[MemberData(
		nameof(RequestBuilderTestData.ValidPayloads),
		MemberType = typeof(RequestBuilderTestData)
	)]
	public async Task WithJSON_WhenSet_AssignJsonContent(object payload)
	{
		var request = new RequestBuilder().WithJSON(payload).Build();

		Assert.NotNull(request.Content);
		var actualPayload = await request.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);

		var expectedJson = JObject.FromObject(payload);
		var actualJson = JsonConvert.DeserializeObject(actualPayload);

		Assert.NotSame(expectedJson, actualJson);
		Assert.Equal(expectedJson, actualJson);
	}

	[Theory]
	[MemberData(
		nameof(RequestBuilderTestData.ValidQueryParameters),
		MemberType = typeof(RequestBuilderTestData)
	)]
	public void SetParameter_WhenSet_AssignQueryValues(Dictionary<string, string> parameters)
	{
		var builder = new RequestBuilder();

		foreach ((var key, var value) in parameters)
			builder.SetParameter(key, value);

		var request = builder.Build();

		Assert.NotNull(request.RequestUri);
		var query = HttpUtility.ParseQueryString(request.RequestUri.Query);

		Assert.Equal(parameters.Count, query.Count);

		foreach (var key in parameters.Keys)
			Assert.Equal(parameters[key], query[key]);
	}

	[Fact]
	public void SetParameter_WhenKeyNull_AssignNoQueryValue()
	{
		var request = new RequestBuilder().SetParameter(null, "?!?").Build();

		Assert.NotNull(request.RequestUri);
		var query = HttpUtility.ParseQueryString(request.RequestUri.Query);

		Assert.Empty(query);
	}

	[Fact]
	public void SetParameter_WhenValueNull_AssignEmptyQueryValue()
	{
		const string KEY = "very-important";

		var request = new RequestBuilder().SetParameter(KEY, null).Build();

		Assert.NotNull(request.RequestUri);
		var query = HttpUtility.ParseQueryString(request.RequestUri.Query);

		Assert.Single(query);
		Assert.Equal(string.Empty, query[KEY]);
	}

	[Theory]
	[MemberData(
		nameof(RequestBuilderTestData.ValidPathParameters),
		MemberType = typeof(RequestBuilderTestData)
	)]
	public void SetPathParameter_WhenSet_ReplacePathParams(
		string endpoint,
		Dictionary<string, string> pathParameters
	)
	{
		var builder = new RequestBuilder()
			.ToUri(new Uri("https://www.google.com"))
			.ToEndpoint(endpoint);

		var expectedPath = endpoint;

		foreach (var key in pathParameters.Keys)
			expectedPath = expectedPath.Replace("{" + key + "}", pathParameters[key]);

		foreach ((var key, var value) in pathParameters)
			builder.SetPathParameter(key, value);

		var request = builder.Build();

		Assert.NotNull(request.RequestUri);
		Assert.Equal(expectedPath, request.RequestUri.AbsolutePath);
	}

	[Fact]
	public void SetPathParameter_WhenSetTwice_ReplacePathParam()
	{
		const string KEY = "UUID";
		const string ENDPOINT = $"/test/{{{KEY}}}/foo";

		const string FIRST_VALUE = "ABC";
		const string LAST_VALUE = "CBA";

		var builder = new RequestBuilder()
			.ToUri(new Uri("https://www.google.com"))
			.ToEndpoint(ENDPOINT);

		var expectedPath = ENDPOINT.Replace($"{{{KEY}}}", LAST_VALUE);

		builder.SetPathParameter(KEY, FIRST_VALUE);
		builder.SetPathParameter(KEY, LAST_VALUE);

		var request = builder.Build();

		Assert.NotNull(request.RequestUri);
		Assert.Equal(expectedPath, request.RequestUri.AbsolutePath);
	}

	[Fact]
	public void Copy_WhenCopied_ReturnNewInstance()
	{
		var originalBuilder = new RequestBuilder();
		var copiedBuilder = new RequestBuilder(originalBuilder);

		Assert.NotSame(copiedBuilder, originalBuilder);
	}

	[Fact]
	public void Copy_WhenCopiedAndModified_OriginalStaysUntouched()
	{
		var originalBuilder = new RequestBuilder().Post();
		var copiedBuilder = new RequestBuilder(originalBuilder).Get();

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.Equal(HttpMethod.Post, originalRequest.Method);
		Assert.Equal(HttpMethod.Get, copiedRequest.Method);
	}

	[Fact]
	public void Copy_WhenCopiedWithMethod_ReturnNewInstanceWithMethod()
	{
		var originalBuilder = new RequestBuilder().Post();
		var copiedBuilder = new RequestBuilder(originalBuilder);

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.Equal(originalRequest.Method, copiedRequest.Method);
	}

	[Fact]
	public void Copy_WhenCopiedWithAuth_ReturnNewInstanceWithAuth()
	{
		var originalBuilder = new RequestBuilder().WithAuth("DDD");
		var copiedBuilder = new RequestBuilder(originalBuilder);

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.NotNull(originalRequest.Headers.Authorization);
		Assert.NotNull(copiedRequest.Headers.Authorization);
		Assert.Equal(
			originalRequest.Headers.Authorization.Parameter,
			copiedRequest.Headers.Authorization.Parameter
		);
	}

	[Fact]
	public async Task Copy_WhenCopiedWithContent_ReturnNewInstanceWithContent()
	{
		var payload = new
		{
			test = 1,
			test2 = 2,
			test3 = 3,
			test4 = 4,
		};

		var originalBuilder = new RequestBuilder().WithJSON(payload);
		var copiedBuilder = new RequestBuilder(originalBuilder);

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.NotNull(originalRequest.Content);
		var originalPayload = await originalRequest.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);

		Assert.NotNull(copiedRequest.Content);
		var copiedPayload = await copiedRequest.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);

		var expectedJson = JObject.FromObject(payload);
		var actualOriginalPayload = JsonConvert.DeserializeObject(originalPayload);
		var actualCopiedPayload = JsonConvert.DeserializeObject(copiedPayload);

		Assert.Equal(expectedJson, actualOriginalPayload);
		Assert.Equal(expectedJson, actualCopiedPayload);
		Assert.NotSame(actualOriginalPayload, actualCopiedPayload);
	}

	[Fact]
	public void Copy_WhenCopiedWithParameter_ReturnNewInstanceWithParameter()
	{
		const string PARAMETER_KEY = "cursor";
		const string PARAMETER_VALUE = "abc";

		var originalBuilder = new RequestBuilder().SetParameter(PARAMETER_KEY, PARAMETER_VALUE);
		var copiedBuilder = new RequestBuilder(originalBuilder);

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.NotNull(originalRequest.RequestUri);
		var originalParameter = HttpUtility
			.ParseQueryString(originalRequest.RequestUri.Query)
			.Get(PARAMETER_KEY);
		Assert.Equal(PARAMETER_VALUE, originalParameter);

		Assert.NotNull(copiedRequest.RequestUri);
		var copiedParameter = HttpUtility
			.ParseQueryString(copiedRequest.RequestUri.Query)
			.Get(PARAMETER_KEY);
		Assert.Equal(PARAMETER_VALUE, copiedParameter);
	}

	[Fact]
	public void Copy_WhenCopiedWithNullKeyParameter_ReturnNewInstanceWithNullKeyParameter()
	{
		const string PARAMETER_VALUE = "abc";

		var originalBuilder = new RequestBuilder().SetParameter(null, PARAMETER_VALUE);
		var copiedBuilder = new RequestBuilder(originalBuilder);

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.NotNull(originalRequest.RequestUri);
		var originalParameter = HttpUtility
			.ParseQueryString(originalRequest.RequestUri.Query)
			.Get(null);
		Assert.Null(originalParameter);

		Assert.NotNull(copiedRequest.RequestUri);
		var copiedParameter = HttpUtility
			.ParseQueryString(copiedRequest.RequestUri.Query)
			.Get(null);
		Assert.Null(copiedParameter);
	}

	[Fact]
	public void Copy_WhenCopiedWithPathParameter_ReturnNewInstanceWithPathParameter()
	{
		const string PATH_KEY = "UUID";
		const string PATH_VALUE = "lethal-company";
		const string EXPECTED_PATH = $"/api/community/{PATH_VALUE}/user";

		var originalBuilder = new RequestBuilder()
			.ToUri(new Uri("https://www.google.com"))
			.ToEndpoint($"/api/community/{{{PATH_KEY}}}/user")
			.SetPathParameter(PATH_KEY, PATH_VALUE);
		var copiedBuilder = new RequestBuilder(originalBuilder);

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.NotNull(originalRequest.RequestUri);
		Assert.Equal(EXPECTED_PATH, originalRequest.RequestUri.AbsolutePath);

		Assert.NotNull(copiedRequest.RequestUri);
		Assert.Equal(EXPECTED_PATH, copiedRequest.RequestUri.AbsolutePath);
	}
}
