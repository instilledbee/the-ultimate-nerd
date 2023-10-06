Title: Serialize a C# Object into an HTTP Query String
Published: 10/06/2023
Tags: [.NET, C#, REST APIs]
---

Sometimes you need to create a C# class that encapsulates a request parameter, say, for a REST API call. 

For example, given the following class definition:

```
public class MyRequestParameters
{
    public string Foo { get; set; }
    public int? Bar { get; set; }
    public bool Baz { get; set; }
}
```

It's relatively easy to use this object for POST requests, e.g. with `HttpClient.PostAsync()`, like so:

```
var myRequest = new MyRequestParameters()
{
    Foo = "someValue",
    Bar = 1,
    Baz = true
};

var stringContent = new StringContent(JsonSerializer.Serialize(myRequest));

// assume httpClient was injected before
var response = await httpClient.PostAsync("https://www.example.com", stringContent)
```

But what if you need to use `MyRequestParameters` as a parameter in a GET API call?

One way to do it would be to build the URI step-by-step with the built-in `UriBuilder` class:
```
var builder = new UriBuilder("https://example.com");
builder.Port = -1;
var query = HttpUtility.ParseQueryString(builder.Query);
query["foo"] = myRequest.Foo;
query["bar"] = myRequest.Bar;
builder.Query = query.ToString();
string url = builder.ToString();

// output = https://example.com/?foo=someValue&bar=1&baz=True
```

It's a bit tedious though, considering that you have to manually go through each of `MyRequestParameters` properties. Another caveat is that if one of the properties in the object is `null`, it still gets added to the resulting query string, which might not necessarily be what you would want:

```
query["foo"] = null;
query["bar"] = null;
builder.Query = query.ToString();
string url = builder.ToString();

// output = https://example.com/?foo=&bar=&baz=True
```

Another issue is that, if the property names change in `MyRequestParameters`, something is added, or removed, you would have to go back to your URI building code and ensure it's updated as well.

If you prefer an approach that automatically goes through an object's properties (so you don't have to manually update any URI builders), and omits `null` values from the final query string, **use this snippet**:

```
/// <summary>
/// Serializes this object's properties into a query string
/// </summary>
public static string ToQueryParams(this object obj) 
{
    var paramStrings = new List<string>();
    
    foreach (var p in obj.GetType().GetProperties()) 
    {
        var value = Convert.ToString(p.GetValue(obj));
        
        if (value != null) 
            paramStrings.Add($"{p.Name}={value}");
    }
    
    if (!paramStrings.Any())
        return string.Empty;

    return $"?{string.Join("&", paramStrings)}";
}
```

The `ToQueryParams()` method is an extension method for `object`, meaning it can be used on any class once included in your project. It iterates through each property, via reflection, and outputs the parameter part of the query string, which can then be concatenated with the final URL as needed, e.g.:

```
var queryParams = myRequest.ToQueryParams();
var baseUrl = "https://example.com";

var finalUrl = $"{baseUrl}{queryParams}";

// output = https://example.com?Foo=someValue&Bar=1&Baz=True
```

---

### Bonus #1: Make parameter names camelCase

Consider if we add the following property to `MyRequestParameters`:

```
public class MyRequestParameters
{
    // ...
    public string CamelCasePlease { get; set; }
}
```

What if we want `CamelCasePlease`, and other properties, to be in camelCase? (e.g. `camelCasePlease=blah`)

Let's add another extension method! This time for `string`:

```
public static string ToQueryParams(this object obj) 
{
    var paramStrings = new List<string>();
    
    foreach (var p in obj.GetType().GetProperties()) 
    {
        var value = Convert.ToString(p.GetValue(obj));
        
        if (value != null) 
            paramStrings.Add($"{p.Name.ToCamelCase()}={value}"); // use .ToCamelCase() here!
    }
    
    if (!paramStrings.Any())
        return string.Empty;

    return $"?{string.Join("&", paramStrings)}";
}

public static string ToCamelCase(this string s) 
{
    return $"{char.ToLowerInvariant(s[0])}{s.Substring(1)}";
}
```

The method should be relatively straightforward - it converts the first character in a string to lower case. Well, this assumes that the property names in your classes are PascalCase to begin with!

---
### Bonus #2: `bool` values in lower case please

What if we need our `bool` property values to be in lower case? e.g. `True` should be concatenated as `true`. We can do that by adding a type check in `ToQueryParams()` foreach loop.

```
public static string ToQueryParams(this object obj) 
{
    var paramStrings = new List<string>();
    
    foreach (var p in obj.GetType().GetProperties()) 
    {
        var value = Convert.ToString(p.GetValue(obj));

        // if it's a boolean, let's convert the value to lower case
        if (p.PropertyType == typeof(bool))
            value = value.ToLower();
        
        if (value != null) 
            paramStrings.Add($"{p.Name.ToCamelCase()}={value}");
    }
    
    if (!paramStrings.Any())
        return string.Empty;

    return $"?{string.Join("&", paramStrings)}";
}
```
