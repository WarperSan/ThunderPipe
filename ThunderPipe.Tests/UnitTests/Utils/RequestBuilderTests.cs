using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThunderPipe.Utils;

// ReSharper disable InconsistentNaming

namespace ThunderPipe.Tests.UnitTests.Utils;

// TODO: Redo all tests to match proper syntax
// TODO: Get 100% coverage on RequestBuilder
public class RequestBuilderTests
{
	public static IEnumerable<object[]> SetMethodData =>
		new List<object[]>
		{
			new object[] { HttpMethod.Get },
			new object[] { HttpMethod.Post },
			new object[] { HttpMethod.Put },
		};

	[Theory]
	[MemberData(nameof(SetMethodData))]
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
				throw new NotImplementedException();
		}

		var request = builder.Build();
		Assert.Equal(method, request.Method);
	}

	[Theory]
	[InlineData("https://www.google.com")]
	[InlineData("https://www.google.com/?a=1")]
	[InlineData("https://google.com")]
	public void ToUri_WhenSet_AssignUri(string url)
	{
		var uri = new Uri(url);
		var request = new RequestBuilder().ToUri(uri).Build();

		Assert.NotNull(request.RequestUri);
		Assert.Equal(uri.AbsoluteUri, request.RequestUri.AbsoluteUri);
	}

	[Theory]
	[InlineData("/apple/com")]
	[InlineData("/https/www/google/com")]
	[InlineData("/api/experimental/submission/package/")]
	public void ToEndpoint_WhenSet_AssignEndpoint(string endpoint)
	{
		var uri = new Uri("https://www.google.com");
		var request = new RequestBuilder().ToUri(uri).ToEndpoint(endpoint).Build();

		Assert.NotNull(request.RequestUri);
		Assert.Equal(endpoint, request.RequestUri.AbsolutePath);
	}

	[Theory]
	[InlineData("vcgbcfhjhgfcxv")]
	[InlineData("vcgfrytahdusbgysahusjdhuagshudjahusdou2 betg2")]
	[InlineData("tss_ nbvcgfhtyjgbgf7asdfgkubeg17y8")]
	[InlineData("uwu this is a very secure password")]
	public void WithAuth_WhenSet_AssignAuthToken(string token)
	{
		var request = new RequestBuilder().WithAuth(token).Build();

		Assert.NotNull(request.Headers.Authorization);
		Assert.Equal(token, request.Headers.Authorization.Parameter);
	}

	public static IEnumerable<object[]> WithJSON_WhenSet_AssignJsonContent_Data =>
		new List<object[]>
		{
			new object[] { new { baba = 2 } },
			new object[] { new { ababa = (string[])["1", "2"] } },
		};

	[Theory]
	[MemberData(nameof(WithJSON_WhenSet_AssignJsonContent_Data))]
	public async Task WithJSON_WhenSet_AssignJsonContent(object payload)
	{
		var request = new RequestBuilder().WithJSON(payload).Build();

		Assert.NotNull(request.Content);
		var actualPayload = await request.Content.ReadAsStringAsync();

		var expectedJson = JObject.FromObject(payload);
		var actualJson = JsonConvert.DeserializeObject(actualPayload);

		Assert.NotSame(expectedJson, actualJson);
		Assert.Equal(expectedJson, actualJson);
	}

	public static IEnumerable<object[]> SetParameter_WhenSet_AssignQueryValues_Data =>
		new List<object[]>
		{
			new object[] { new Dictionary<string, string> { ["test"] = "1" } },
			new object[]
			{
				new Dictionary<string, string>
				{
					["test2"] = "1",
					["test3"] = "2",
					["uwu"] = "OwO",
				},
			},
		};

	[Theory]
	[MemberData(nameof(SetParameter_WhenSet_AssignQueryValues_Data))]
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

	public static IEnumerable<object[]> SetPathParameter_WhenSet_ReplacePathParams_Data =>
		new List<object[]>
		{
			new object[]
			{
				"/test/{UUID}/foo",
				new Dictionary<string, string> { ["UUID"] = "123-321" },
			},
			new object[]
			{
				"/foo/{BAR}/foo/{ID}/foo",
				new Dictionary<string, string> { ["BAR"] = "123", ["ID"] = "321" },
			},
		};

	[Theory]
	[MemberData(nameof(SetPathParameter_WhenSet_ReplacePathParams_Data))]
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
	public void Copy_WhenCopied_ReturnNewInstance()
	{
		var originalBuilder = new RequestBuilder();
		var copiedBuilder = originalBuilder.Copy();

		Assert.NotSame(copiedBuilder, originalBuilder);
	}

	[Fact]
	public void Copy_WhenCopiedAndModified_OriginalStaysUntouched()
	{
		var originalBuilder = new RequestBuilder().Post();
		var copiedBuilder = originalBuilder.Copy().Get();

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.Equal(HttpMethod.Post, originalRequest.Method);
		Assert.Equal(HttpMethod.Get, copiedRequest.Method);
	}

	[Fact]
	public void Copy_WhenCopiedWithMethod_ReturnNewInstanceWithMethod()
	{
		var originalBuilder = new RequestBuilder().Post();
		var copiedBuilder = originalBuilder.Copy();

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.Equal(originalRequest.Method, copiedRequest.Method);
	}

	[Fact]
	public void Copy_WhenCopiedWithAuth_ReturnNewInstanceWithAuth()
	{
		var originalBuilder = new RequestBuilder().WithAuth("DDD");
		var copiedBuilder = originalBuilder.Copy();

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
		// Arrange
		var payload = new
		{
			test = 1,
			test2 = 2,
			test3 = 3,
			test4 = 4,
		};

		var originalBuilder = new RequestBuilder().WithJSON(payload);
		var copiedBuilder = originalBuilder.Copy();

		// Act
		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		// Assert
		Assert.NotNull(originalRequest.Content);
		var originalPayload = await originalRequest.Content.ReadAsStringAsync();

		Assert.NotNull(copiedRequest.Content);
		var copiedPayload = await copiedRequest.Content.ReadAsStringAsync();

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
		var copiedBuilder = originalBuilder.Copy();

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
	public void Copy_WhenCopiedWithPathParameter_ReturnNewInstanceWithPathParameter()
	{
		const string PATH_KEY = "UUID";
		const string PATH_VALUE = "lethal-company";
		const string EXPECTED_PATH = $"/api/community/{PATH_VALUE}/user";

		var originalBuilder = new RequestBuilder()
			.ToUri(new Uri("https://www.google.com"))
			.ToEndpoint($"/api/community/{{{PATH_KEY}}}/user")
			.SetPathParameter(PATH_KEY, PATH_VALUE);
		var copiedBuilder = originalBuilder.Copy();

		var originalRequest = originalBuilder.Build();
		var copiedRequest = copiedBuilder.Build();

		Assert.NotNull(originalRequest.RequestUri);
		Assert.Equal(EXPECTED_PATH, originalRequest.RequestUri.AbsolutePath);

		Assert.NotNull(copiedRequest.RequestUri);
		Assert.Equal(EXPECTED_PATH, copiedRequest.RequestUri.AbsolutePath);
	}
}
