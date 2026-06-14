# Plugins & Functions

A **plugin** is a named collection of `KernelFunction`s the kernel can invoke — the unit of capability you register, share, and expose to the model for tool calling. Functions come in two flavors: **native functions** (C# methods decorated with `[KernelFunction]`) and **prompt functions** (templated prompts). The kernel treats both uniformly as `KernelFunction`s, which is why the model can call either through function calling.

---

## Native functions and `[KernelFunction]`

`[KernelFunction]` marks a method as callable by the kernel and exposable to the LLM. SK reflects over the method to build a `KernelFunction` descriptor — function name (overridable), description (from `[Description]`), and parameter schema — and that metadata becomes the **JSON tool definition the model sees**.

```csharp
using System.ComponentModel;
using Microsoft.SemanticKernel;

public sealed class WeatherPlugin
{
    private readonly IWeatherClient _client;
    public WeatherPlugin(IWeatherClient client) => _client = client;   // DI-injected

    [KernelFunction("get_current_temperature")]
    [Description("Gets the current temperature in Celsius for a city.")]
    public async Task<double> GetTemperatureAsync(
        [Description("City name, e.g. 'Seattle'")] string city,
        CancellationToken ct = default)
        => await _client.GetTempCelsiusAsync(city, ct);
}
```

The `[Description]` attributes are **not documentation — they are the tool schema** sent to the model. Bad descriptions cause bad tool selection. The model never sees your code, only the schema, so plugin design mirrors good API design: clear `snake_case` verb-noun names, precise parameter descriptions, and narrow single-purpose functions. Methods can be sync or async, return primitives, strings, or complex objects (serialized to JSON), and may accept the `Kernel` or `KernelArguments` as parameters.

### Registering native plugins

```csharp
var builder = Kernel.CreateBuilder();
builder.Services.AddSingleton<IWeatherClient, OpenMeteoClient>();
Kernel kernel = builder.Build();

// AddFromType<T> resolves T from the kernel's service provider (honors DI)
kernel.Plugins.AddFromType<WeatherPlugin>("Weather", kernel.Services);

// AddFromObject bypasses DI — you control construction
kernel.Plugins.AddFromObject(new WeatherPlugin(new OpenMeteoClient()), "Weather");

// Build a plugin from ad-hoc functions
kernel.Plugins.AddFromFunctions("Math",
    [kernel.CreateFunctionFromMethod((double a, double b) => a + b, "add")]);
```

**Production gotcha:** `AddFromType<T>()` resolves `T` from the service provider, so constructor-injected dependencies must be registered *before* `Build()`. `AddFromObject(new WeatherPlugin(...))` bypasses DI and loses scoped lifetimes — a classic source of `DbContext` / `HttpClient` lifetime bugs in web apps. Prefer `AddFromType` with proper DI, or build a per-request kernel.

---

## Prompt (semantic) functions

A prompt function is defined by a prompt template plus configuration; "executing" it renders the template and calls the LLM, producing natural-language or structured output. Native functions are precise, testable, and fast; prompt functions handle fuzzy, language-centric tasks (summarize, classify, rewrite).

```csharp
// Inline from code
var summarize = kernel.CreateFunctionFromPrompt(
    "Summarize in {{$count}} bullets:\n{{$text}}",
    new OpenAIPromptExecutionSettings { MaxTokens = 200, Temperature = 0.2 });

var result = await kernel.InvokeAsync(summarize, new KernelArguments
{
    ["count"] = 3,
    ["text"]  = longDocument
});
```

Prompt functions can also load from the classic on-disk layout (a folder with `skprompt.txt` + `config.json`) or from `.prompty` YAML — see [04-prompt-templates.md](04-prompt-templates.md). This externalizes prompts so prompt engineers iterate on files while developers consume them as functions.

```csharp
// Loads Plugins/Summarize/skprompt.txt + config.json
kernel.ImportPluginFromPromptDirectory("Plugins/Summarize", "Summarize");
```

A prompt template can even invoke a native function inline to fetch data, then reason over it — the kernel's power is unifying deterministic code and probabilistic language operations under one invocation model.

---

## `KernelArguments`

`KernelArguments` is a dictionary-like bag (`IDictionary<string, object?>`) that carries input variables **and** execution settings into an invocation. Template placeholders like `{{$city}}` bind to keys; native parameters bind by name. It also carries `PromptExecutionSettings`, letting you set temperature, max tokens, and `FunctionChoiceBehavior` per call.

```csharp
var args = new KernelArguments(
    new AzureOpenAIPromptExecutionSettings { Temperature = 0.2 })
{
    ["city"]  = "Seattle",
    ["units"] = "metric"
};

var result = await kernel.InvokeAsync(myFunction, args);
```

Because it is a single object threaded through the pipeline, arguments are **visible to filters** (see [07-filters-and-middleware.md](07-filters-and-middleware.md)), which can inspect or mutate them before execution. You can supply multiple service-specific settings keyed by `serviceId` for multi-model setups. It keeps the kernel itself stateless across calls.

---

## Legacy "Skills" vs current "Plugins"

In the **2023-era SK API**, the unit was called a **Skill** (`ISKFunction`, `RegisterSemanticFunction`, the `Skills` directory). Microsoft renamed **Skills → Plugins** to align with the OpenAI/ChatGPT "plugins" ecosystem. The runtime concept is identical; only names and namespaces changed.

| Legacy (≤2023) | Current (.NET, 2024–2026) |
|---|---|
| Skill | Plugin |
| `ISKFunction` | `KernelFunction` |
| `IKernel` | `Kernel` (concrete) |
| `SKContext` | `KernelArguments` + `FunctionResult` |
| `kernel.ImportSkill(...)` | `kernel.Plugins.AddFromType/AddFromObject/AddFromFunctions` |
| Semantic function | Prompt function |
| `[SKFunction]` | `[KernelFunction]` |
| `skprompt.txt` + `config.json` | Same on-disk convention, loaded via `ImportPluginFromPromptDirectory` |

**Production gotcha:** Many blog posts and even some older docs still show `IKernel` / `ImportSkill` / `SKContext`. Copy-pasting that into a 2026 project won't compile. Filter to `KernelFunction` / `KernelArguments` examples — that's the GA shape.

---

## Importing plugins from OpenAPI

SK can turn an OpenAPI (Swagger) spec into a plugin so the model can call REST endpoints as functions — powerful for exposing existing microservices without writing native wrappers.

```csharp
await kernel.ImportPluginFromOpenApiAsync(
    pluginName: "Tickets",
    uri: new Uri("https://api.example.com/openapi.json"),
    executionParameters: new OpenApiFunctionExecutionParameters
    {
        AuthCallback = async (req, ct) =>
            req.Headers.Authorization = new("Bearer", await GetTokenAsync())
    });
```

Cautions: large specs create many tools that overwhelm the model and the context window — filter to needed operations; secure the auth callback (managed identity / OBO); and gate destructive operations (DELETE/POST) behind an approval filter.

---

## Interview angle

> "What turns a C# method into something the LLM can call?" The `[KernelFunction]` attribute registers it; the method signature plus `[Description]` attributes are serialized into a JSON tool/function schema that the chat completion service advertises to the model. The model returns tool calls; SK maps them back to the method. If asked "skill vs plugin," the expert answer is "same thing — 'skill' is the deprecated name; the *interesting* distinction is native vs prompt functions, and how both surface to function calling."

---

**Next:** [Function Calling](03-function-calling.md) — how the model actually decides to invoke your functions.
