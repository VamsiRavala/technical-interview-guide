# Cross-Site Scripting (XSS)

Cross-Site Scripting (XSS) is a security vulnerability that enables a cyberattacker to place client side scripts (usually JavaScript) into web pages. When other users load affected pages, the cyberattacker's scripts run, enabling the cyberattacker to steal cookies and session tokens, change the contents of the web page through DOM manipulation, or redirect the browser to another page. XSS vulnerabilities generally occur when an application takes user input and outputs it to a page without validating, encoding or escaping it.

This article applies primarily to ASP.NET Core MVC with views, Razor Pages, and other apps that return HTML that may be vulnerable to XSS. Web APIs that return data in the form of HTML, XML, or JSON can trigger XSS attacks in their client apps if they don't properly sanitize user input, depending on how much trust the client app places in the API. For example, if an API accepts user-generated content and returns it in an HTML response, a cyberattacker could inject malicious scripts into the content that executes when the response is rendered in the user's browser.

To prevent XSS attacks, web APIs should implement input validation and output encoding. Input validation ensures that user input meets expected criteria and doesn't include malicious code. Output encoding ensures that any data returned by the API is properly sanitized so that it can't be executed as code by the user's browser.


## How to prevent it (framework-agnostic)

1. **Encode on output (by context).** Encode *when rendering*, and use the right encoder for the context:
   - HTML, HTML attribute, JavaScript string, URL component.
   - Don’t “HTML-encode on input”; store raw data and encode at render time.

2. **Sanitize only when you must render user-authored HTML.** For WYSIWYG/CMS/comment content, use a robust sanitizer to whitelist tags/attributes.

3. **Set a strong Content-Security-Policy (CSP).** Use nonces or hashes for scripts, block `unsafe-inline`, disallow `object`. CSP won’t fix XSS by itself but limits impact.

4. **Avoid dangerous sinks.** Prefer `textContent` / safe templating over `innerHTML` and never use `eval`/`new Function` unless absolutely necessary.

5. **Security headers.** Prefer CSP; `X-XSS-Protection` is obsolete and should not be relied upon.

---

## What ASP.NET Core gives you

### 1) Razor auto-encodes by default
```cshtml
@* In Razor views, expressions are HTML-encoded automatically *@
<p>@Model.Comment</p>

@* Avoid using Html.Raw on untrusted content *@
@Html.Raw(safeButSanitizedHtml)  @* Only after sanitization *@
```

### 2) First-class encoders in `System.Text.Encodings.Web`
```csharp
using System.Text.Encodings.Web;

public class NotesService
{
    private readonly HtmlEncoder _html;
    private readonly JavaScriptEncoder _js;

    public NotesService(HtmlEncoder html, JavaScriptEncoder js)
    { 
        _html = html; 
        _js = js; 
    }

    public string SafeHtml(string s) => _html.Encode(s);
    public string SafeJsString(string s) => _js.Encode(s);
}
```

The key encoders are:
- `HtmlEncoder`
- `JavaScriptEncoder`
- `UrlEncoder`

### 3) JSON serialization is XSS-aware by default
`System.Text.Json` escapes HTML-significant characters (`<`, `>`, `&`) by default so that JSON is safer if embedded in HTML. Avoid turning this off with `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` unless you’re certain the JSON will **never** be embedded in HTML.

```csharp
// Keep the safe default:
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // DON'T do this unless you really know why:
        // o.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });
```

### 4) Sanitization (when you must render HTML)
ASP.NET Core does not include a built-in sanitizer (by design). Use a well-maintained library such as **Ganss.XSS (HtmlSanitizer)** to whitelist tags/attributes and strip script content.

```csharp
using Ganss.XSS;

var sanitizer = new HtmlSanitizer();
sanitizer.AllowedTags.Add("b");
sanitizer.AllowedAttributes.Add("class");

string safeHtml = sanitizer.Sanitize(untrustedHtml);

// Now and only now — if you must render raw HTML:
@Html.Raw(safeHtml)
```

### 5) Content-Security-Policy (CSP) middleware
Add CSP headers with a library like **NWebsec.AspNetCore**. Use nonces for scripts, block inline scripts, and disable plugin objects.

```csharp
// Program.cs (after installing NWebsec.AspNetCore.* packages)
app.UseCsp(options =>
{
    options.DefaultSources(s => s.Self());
    options.ScriptSources(s => s.Self().Https().WithNonce());
    options.ObjectSources(s => s.None());
    options.FrameAncestors(s => s.Self());
});
```

> **Note on legacy AntiXSS**: `System.Web.Security.AntiXss` was for classic ASP.NET. In ASP.NET Core, prefer Razor’s default encoding and the encoders in `System.Text.Encodings.Web` rather than trying to port AntiXSS.

---

## Quick checklist (copy/paste)

- **Views (Razor):** Rely on auto-encoding; never `Html.Raw` unsanitized input.  
- **Controllers/Services:** Use `HtmlEncoder` / `JavaScriptEncoder` / `UrlEncoder` for manual encoding in the right context.  
- **JSON:** Keep `System.Text.Json` defaults; avoid `UnsafeRelaxedJsonEscaping` unless you’re *certain* about the embedding context.  
- **Rich-text inputs:** Sanitize with `HtmlSanitizer` before rendering; store raw data, sanitize at render time.  
- **Front-end:** Avoid `innerHTML`/`eval`; prefer `textContent` and safe templating.  
- **Headers:** Deploy a strict CSP with nonces/hashes; don’t rely on `X-XSS-Protection`.

---

## Want a tailored snippet?

Tell me your stack (Razor Pages, MVC, Blazor, or SPA + Web API) and I’ll drop in a CSP and sanitizer config that fits it.
