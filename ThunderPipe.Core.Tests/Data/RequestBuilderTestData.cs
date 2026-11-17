using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.Data;

/// <summary>
/// Test data for <see cref="RequestBuilder"/> tests
/// </summary>
public class RequestBuilderTestData
{
	/// <summary>
	/// List of valid <see cref="HttpMethod"/>
	/// </summary>
	public static TheoryData<HttpMethod> ValidMethods =>
		[HttpMethod.Get, HttpMethod.Post, HttpMethod.Put];

	/// <summary>
	/// List of valid base URIs
	/// </summary>
	public static TheoryData<string> ValidUris =>
		[
			"https://www.google.com",
			"https://www.google.com/?a=1",
			"https://google.com",
			"http://localhost:8080",
			"https://192.168.1.1/api",
		];

	/// <summary>
	/// List of valid API endpoints
	/// </summary>
	public static TheoryData<string> ValidEndpoints =>
		[
			"/apple/com",
			"/https/www/google/com",
			"/api/experimental/submission/package/",
			"/api/v1/users",
			"/search",
		];

	/// <summary>
	/// List of valid authentication token strings
	/// </summary>
	public static TheoryData<string> ValidTokens =>
		[
			"vcgbcfhjhgfcxv",
			"vcgfrytahdusbgysahusjdhuagshudjahusdou2 betg2",
			"tss_ nbvcgfhtyjgbgf7asdfgkubeg17y8",
			"uwu this is a very secure password",
		];

	/// <summary>
	/// List of valid payloads
	/// </summary>
	public static TheoryData<object> ValidPayloads =>
		[
			new { baba = 2 },
			new { ababa = (string[])["1", "2"] },
			new { name = "test", value = "" },
			new { nested = new { deep = true } },
			new { empty = Array.Empty<string>() },
		];

	/// <summary>
	/// List of valid query parameters
	/// </summary>
	public static TheoryData<Dictionary<string, string>> ValidQueryParameters =>
		[
			new Dictionary<string, string> { ["test"] = "1" },
			new Dictionary<string, string>
			{
				["test2"] = "1",
				["test3"] = "2",
				["uwu"] = "OwO",
			},
			new Dictionary<string, string>(),
			new Dictionary<string, string> { ["key with spaces"] = "value%20encoded" },
			new Dictionary<string, string> { ["very_long_parameter_name"] = "short" },
		];

	/// <summary>
	/// List of valid path templates with parameter replacement maps
	/// </summary>
	public static TheoryData<string, Dictionary<string, string>> ValidPathParameters =>
		new()
		{
			{
				"/test/{UUID}/foo",
				new Dictionary<string, string> { ["UUID"] = "123-321" }
			},
			{
				"/foo/{BAR}/foo/{ID}/foo",
				new Dictionary<string, string> { ["BAR"] = "123", ["ID"] = "321" }
			},
			{ "/static/path", new Dictionary<string, string>() },
			{
				"/{A}/{B}/{C}/{D}",
				new Dictionary<string, string>
				{
					["A"] = "1",
					["B"] = "2",
					["C"] = "3",
					["D"] = "4",
				}
			},
			{
				"/path/{name}/end",
				new Dictionary<string, string> { ["name"] = "value_with_underscores" }
			},
		};
}
