# IActionResult

Type: Interface

Represents the contract for an action result in ASP.NET Core.

Any class that implements IActionResult can be returned from a controller action (e.g., OkResult, NotFoundResult, BadRequestResult, etc.).

Typically used when your action can return different result types depending on conditions


# Example
[HttpGet("{id}")]
public IActionResult GetItem(int id)
{
    var item = _repository.Get(id);

    if (item == null)
        return NotFound();  // returns 404

    return Ok(item);        // returns 200 + JSON object
}

# IActionResult<T>

Type: Class (generic)

Introduced in ASP.NET Core 2.1 to improve Web API return type clarity.

Combines the flexibility of IActionResult with the strong typing of T.

Lets you return either:

A specific type T (e.g., your model or DTO), or

An IActionResult (like NotFound() or BadRequest()).

# Example
[HttpGet("welcome")]
public ActionResult<string> Welcome()
{
    return "Hello Laksh, welcome to the API!";
}

[HttpGet("products")]
public ActionResult<List<Product>> GetProducts()
{
    var products = new List<Product>
    {
        new Product { Id = 1, Name = "Laptop" },
        new Product { Id = 2, Name = "Phone" }
    };

    return products;
}

# IActionResult vs Task<IActionResult>

IActionResult → used for synchronous controller actions.

Task<IActionResult> → used for asynchronous controller actions.
