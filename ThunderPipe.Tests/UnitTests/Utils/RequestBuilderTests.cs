using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThunderPipe.Utils;

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

	public static IEnumerable<object[]> SetUriData =>
		new List<object[]>
		{
			new object[] { new Uri("https://www.google.com") },
			new object[] { new Uri("https://www.google.com/?a=1") },
			new object[] { new Uri("https://google.com") },
		};

	[Theory]
	[MemberData(nameof(SetUriData))]
	public void SetUri(Uri expected)
	{
		var request = new RequestBuilder().ToUri(expected).Build();

		Assert.Equal(expected, request.RequestUri);
	}

	public static IEnumerable<object[]> SetEndpointData =>
		new List<object[]>
		{
			new object[] { "/apple/com" },
			new object[] { "/https/www/google/com" },
			new object[] { "/api/experimental/submission/package/" },
		};

	[Theory]
	[MemberData(nameof(SetEndpointData))]
	public void SetEndpoint(string endpoint)
	{
		var uri = new Uri("https://www.google.com");
		var request = new RequestBuilder().ToUri(uri).ToEndpoint(endpoint).Build();

		Assert.Equal(endpoint, request.RequestUri?.AbsolutePath);
	}

	public static IEnumerable<object[]> SetAuthData =>
		new List<object[]>
		{
			new object[] { "vcgbcfhjhgfcxv" },
			new object[] { "vcgfrytahdusbgysahusjdhuagshudjahusdou2 betg2" },
			new object[] { "tss_ nbvcgfhtyjgbgf7asdfgkubeg17y8" },
			new object[] { "uwu this is a very secure password" },
		};

	[Theory]
	[MemberData(nameof(SetAuthData))]
	public void SetAuth(string token)
	{
		var request = new RequestBuilder().WithAuth(token).Build();
		Assert.Equal(token, request.Headers.Authorization?.Parameter);
	}

	public static IEnumerable<object[]> SetJsonContentData =>
		new List<object[]>
		{
			new object[] { new { baba = 2 } },
			new object[] { new { ababa = (string[])["1", "2"] } },
		};

	[Theory]
	[MemberData(nameof(SetJsonContentData))]
	public async Task SetJsonContent(object json)
	{
		var request = new RequestBuilder().WithJSON(json).Build();

		Assert.NotNull(request.Content);
		var payload = await request.Content!.ReadAsStringAsync();

		var expectedJson = JObject.FromObject(json);
		var actualJson = JsonConvert.DeserializeObject(payload);

		Assert.Equal(expectedJson, actualJson);
	}

	public static IEnumerable<object[]> SetParameterData =>
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
	[MemberData(nameof(SetParameterData))]
	public void SetParameter(Dictionary<string, string> parameters)
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

	public static IEnumerable<object[]> SetPathParameterData =>
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
	[MemberData(nameof(SetPathParameterData))]
	public void SetPathParameter(string endpoint, Dictionary<string, string> pathParameters)
	{
		var builder = new RequestBuilder()
			.ToUri(new Uri("https://www.google.com"))
			.ToEndpoint(endpoint);

		foreach ((var key, var value) in pathParameters)
			builder.SetPathParameter(key, value);

		var request = builder.Build();

		Assert.NotNull(request.RequestUri);
		var path = endpoint;

		foreach (var key in pathParameters.Keys)
			path = path.Replace("{" + key + "}", pathParameters[key]);

		Assert.Equal(path, request.RequestUri.AbsolutePath);
	}

	[Fact]
	public void CopyBuilder()
	{
		var firstBuilder = new RequestBuilder();

		firstBuilder.Get();
		firstBuilder.WithAuth("ABC");

		var secondBuilder = firstBuilder.Copy();
		secondBuilder.Post();

		var firstRequest = firstBuilder.Build();
		var secondRequest = secondBuilder.Build();
		var thirdRequest = firstBuilder.Post().WithAuth("DDD").Build();

		Assert.NotSame(firstBuilder, secondBuilder);
		Assert.Equal(HttpMethod.Get, firstRequest.Method);
		Assert.Equal(HttpMethod.Post, secondRequest.Method);
		Assert.Equal(HttpMethod.Post, thirdRequest.Method);
		Assert.Equal("ABC", firstRequest.Headers.Authorization?.Parameter);
		Assert.Equal("ABC", secondRequest.Headers.Authorization?.Parameter);
		Assert.Equal("DDD", thirdRequest.Headers.Authorization?.Parameter);
	}

	[Fact]
	public async Task Copy_WhenHasContent_CopyContent()
	{
		// Arrange
		var payload = new
		{
			test = 1,
			test2 = 2,
			test3 = 3,
			test4 = 4,
		};

		var firstBuilder = new RequestBuilder().WithJSON(payload);

		var secondBuilder = firstBuilder.Copy();

		// Act
		var firstRequest = firstBuilder.Build();
		var secondRequest = secondBuilder.Build();

		// Assert
		Assert.NotSame(firstBuilder, secondBuilder);

		Assert.NotNull(firstRequest.Content);
		Assert.NotNull(secondRequest.Content);

		var firstPayload = await firstRequest.Content!.ReadAsStringAsync();
		var secondPayload = await secondRequest.Content!.ReadAsStringAsync();

		var expectedJson = JObject.FromObject(payload);
		var actualFirstJson = JsonConvert.DeserializeObject(firstPayload);
		var actualSecondJson = JsonConvert.DeserializeObject(secondPayload);

		Assert.Equal(expectedJson, actualFirstJson);
		Assert.Equal(expectedJson, actualSecondJson);
	}
}
