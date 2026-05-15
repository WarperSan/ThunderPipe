using System.Net;
using System.Text;
using Newtonsoft.Json;
using ThunderPipe.Core.Models.Web;

namespace ThunderPipe.Core.Tests.UnitTests.Models;

public class ResponseTests
{
	private static HttpResponseMessage CreateMessage(HttpStatusCode status, object content)
	{
		var jsonPayload = JsonConvert.SerializeObject(content);

		var response = new HttpResponseMessage(status);

		response.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

		return response;
	}

	private record Product
	{
		public required int Id { get; set; }
		public required string Name { get; set; }
		public required decimal Price { get; set; }
	}

	[Fact]
	public async Task CreateResponse_WhenReturnOK_ReturnPayload()
	{
		var expectedProduct = new Product
		{
			Id = 10,
			Name = "ABC",
			Price = 9.99m,
		};

		var rawResponse = CreateMessage(HttpStatusCode.OK, expectedProduct);
		var content = await rawResponse.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);
		var response = Response<Product>.CreateResponse(rawResponse, content);

		Assert.True(response.IsSuccess);
		Assert.Empty(response.Errors);
		Assert.NotNull(response.Data);
		Assert.Equal(expectedProduct, response.Data);
	}

	[Fact]
	public async Task CreateResponse_WhenReturnBadRequestWithObject_ReturnErrors()
	{
		const string CATEGORY_1 = "namespace";
		const string CATEGORY_2 = "non_field_errors";
		const string CATEGORY_3 = "version";

		const string ERROR_1 = "This field is required.";
		const string ERROR_2 = "This field cannot be empty.";
		const string ERROR_3 = "This field should be an integer.";
		const string ERROR_4 = "The server crashed, bro.";

		var errors = new Dictionary<string, string[]>()
		{
			[CATEGORY_1] = [ERROR_1, ERROR_2],
			[CATEGORY_2] = [ERROR_2, ERROR_3],
			[CATEGORY_3] = [ERROR_3, ERROR_1, ERROR_4],
		};

		var rawResponse = CreateMessage(HttpStatusCode.BadRequest, errors);
		var content = await rawResponse.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);
		var response = Response<Product>.CreateResponse(rawResponse, content);

		Assert.False(response.IsSuccess);
		Assert.Null(response.Data);
		Assert.NotEmpty(response.Errors);

		foreach (var error in errors)
		{
			Assert.Contains(error.Key, response.Errors);

			foreach (var value in error.Value)
				Assert.Contains(value, response.Errors[error.Key]);
		}
	}

	[Fact]
	public async Task CreateResponse_WhenReturnBadRequestWithArray_ReturnErrors()
	{
		const string ERROR_1 = "This field is required.";
		const string ERROR_2 = "This field cannot be empty.";
		const string ERROR_3 = "This field should be an integer.";
		const string ERROR_4 = "The server crashed, bro.";

		var errors = new[] { ERROR_1, ERROR_2, ERROR_3, ERROR_4 };

		var rawResponse = CreateMessage(HttpStatusCode.BadRequest, errors);
		var content = await rawResponse.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);
		var response = Response<Product>.CreateResponse(rawResponse, content);

		Assert.False(response.IsSuccess);
		Assert.Null(response.Data);
		Assert.NotEmpty(response.Errors);
		Assert.Contains(Response<Product>.GLOBAL_ERRORS, response.Errors);

		foreach (var error in errors)
			Assert.Contains(error, response.Errors[Response<Product>.GLOBAL_ERRORS]);
	}

	[Fact]
	public async Task CreateResponse_WhenReturnBadRequestWithOther_ThrowException()
	{
		var rawResponse = CreateMessage(HttpStatusCode.BadRequest, 10);

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			var content = await rawResponse.Content.ReadAsStringAsync(
				TestContext.Current.CancellationToken
			);
			Response<Product>.CreateResponse(rawResponse, content);
		});
	}

	[Fact]
	public async Task CreateResponse_WhenReturnBadRequestWithArrayOfObjects_ReturnErrors()
	{
		// {"a":[{"b": "", "c": ""}, {"d": ""}]}
		const string CATEGORY_1 = "namespace";
		const string CATEGORY_2 = "non_field_errors";
		const string CATEGORY_3 = "version";
		const string CATEGORY_4 = "slime";

		const string ERROR_1 = "This field is required.";
		const string ERROR_2 = "This field cannot be empty.";
		const string ERROR_3 = "Time to get sticky.";

		const string KEY_ERROR_1 = $"{CATEGORY_1}[0].{CATEGORY_2}";
		const string KEY_ERROR_2 = $"{CATEGORY_1}[0].{CATEGORY_3}";
		const string KEY_ERROR_3 = $"{CATEGORY_1}[1].{CATEGORY_4}";

		var errors = new Dictionary<string, Dictionary<string, string>[]>()
		{
			[CATEGORY_1] =
			[
				new Dictionary<string, string>() { [CATEGORY_2] = ERROR_1, [CATEGORY_3] = ERROR_2 },
				new Dictionary<string, string>() { [CATEGORY_4] = ERROR_3 },
			],
		};

		var rawResponse = CreateMessage(HttpStatusCode.BadRequest, errors);
		var content = await rawResponse.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);
		var response = Response<Product>.CreateResponse(rawResponse, content);

		Assert.False(response.IsSuccess);
		Assert.Null(response.Data);
		Assert.NotEmpty(response.Errors);
		Assert.Contains(KEY_ERROR_1, response.Errors);
		Assert.Contains(KEY_ERROR_2, response.Errors);
		Assert.Contains(KEY_ERROR_3, response.Errors);

		Assert.Contains(ERROR_1, response.Errors[KEY_ERROR_1]);
		Assert.Contains(ERROR_2, response.Errors[KEY_ERROR_2]);
		Assert.Contains(ERROR_3, response.Errors[KEY_ERROR_3]);
	}

	[Fact]
	public async Task CreateResponse_WhenReturnBadRequestWithNestedObject_ReturnErrors()
	{
		// {"a":{"b":{"d": "", "e": ""}, "c": ""}}
		const string CATEGORY_1 = "namespace";
		const string CATEGORY_2 = "non_field_errors";
		const string CATEGORY_3 = "version";
		const string CATEGORY_4 = "testing";
		const string CATEGORY_5 = "production";

		const string ERROR_1 = "This field is required.";
		const string ERROR_2 = "This field cannot be empty.";
		const string ERROR_3 = "This field should be an integer.";

		const string KEY_ERROR_1 = $"{CATEGORY_1}.{CATEGORY_2}.{CATEGORY_3}";
		const string KEY_ERROR_2 = $"{CATEGORY_1}.{CATEGORY_2}.{CATEGORY_4}";
		const string KEY_ERROR_3 = $"{CATEGORY_1}.{CATEGORY_5}";

		var errors = new Dictionary<string, dynamic>()
		{
			[CATEGORY_1] = new Dictionary<string, dynamic>()
			{
				[CATEGORY_2] = new Dictionary<string, string>()
				{
					[CATEGORY_3] = ERROR_1,
					[CATEGORY_4] = ERROR_2,
				},
				[CATEGORY_5] = ERROR_3,
			},
		};

		var rawResponse = CreateMessage(HttpStatusCode.BadRequest, errors);
		var content = await rawResponse.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);
		var response = Response<Product>.CreateResponse(rawResponse, content);

		Assert.False(response.IsSuccess);
		Assert.Null(response.Data);
		Assert.NotEmpty(response.Errors);

		Assert.Contains(KEY_ERROR_1, response.Errors);
		Assert.Contains(KEY_ERROR_2, response.Errors);
		Assert.Contains(KEY_ERROR_3, response.Errors);

		Assert.Contains(ERROR_1, response.Errors[KEY_ERROR_1]);
		Assert.Contains(ERROR_2, response.Errors[KEY_ERROR_2]);
		Assert.Contains(ERROR_3, response.Errors[KEY_ERROR_3]);
	}

	[Theory]
	[InlineData(HttpStatusCode.NotFound, "Upload not found")]
	[InlineData(HttpStatusCode.NotFound, "Not found.")]
	[InlineData(HttpStatusCode.Unauthorized, "Authentication credentials were not provided.")]
	[InlineData(HttpStatusCode.MethodNotAllowed, "Method \\\"POST\\\" not allowed.")]
	[InlineData(
		HttpStatusCode.UnsupportedMediaType,
		"Unsupported media type \\\"text/plain\\\" in request."
	)]
	[InlineData(HttpStatusCode.InternalServerError, "Internal server error")]
	public async Task CreateResponse_WhenReturnErrorWithDetails_ReturnDetails(
		HttpStatusCode status,
		string details
	)
	{
		var rawResponse = CreateMessage(status, new { detail = details });

		var content = await rawResponse.Content.ReadAsStringAsync(
			TestContext.Current.CancellationToken
		);
		var response = Response<Product>.CreateResponse(rawResponse, content);

		Assert.False(response.IsSuccess);
		Assert.Null(response.Data);
		Assert.NotEmpty(response.Errors);
		Assert.Contains(Response<Product>.GLOBAL_ERRORS, response.Errors);
		Assert.Contains(details, response.Errors[Response<Product>.GLOBAL_ERRORS]);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)]
	[InlineData(HttpStatusCode.Continue)]
	[InlineData(HttpStatusCode.NotImplemented)]
	[InlineData(HttpStatusCode.Redirect)]
	[InlineData(HttpStatusCode.LoopDetected)]
	public async Task CreateResponse_WhenReturnUnsupported_ThrowException(HttpStatusCode status)
	{
		var expectedProduct = new Product
		{
			Id = 10,
			Name = "ABC",
			Price = 9.99m,
		};

		var rawResponse = CreateMessage(status, expectedProduct);

		await Assert.ThrowsAsync<NotSupportedException>(async () =>
		{
			var content = await rawResponse.Content.ReadAsStringAsync(
				TestContext.Current.CancellationToken
			);
			Response<Product>.CreateResponse(rawResponse, content);
		});
	}
}
