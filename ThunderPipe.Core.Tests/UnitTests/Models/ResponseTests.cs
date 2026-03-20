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
		var response = await Response<Product>.CreateResponse(rawResponse, CancellationToken.None);

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
		var response = await Response<Product>.CreateResponse(rawResponse, CancellationToken.None);

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
		var response = await Response<Product>.CreateResponse(rawResponse, CancellationToken.None);

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
			await Response<Product>.CreateResponse(rawResponse, CancellationToken.None)
		);
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

		var response = await Response<Product>.CreateResponse(rawResponse, CancellationToken.None);

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
			await Response<Product>.CreateResponse(rawResponse, CancellationToken.None)
		);
	}
}
