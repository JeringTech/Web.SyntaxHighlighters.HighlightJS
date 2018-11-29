# Jering.Web.SyntaxHighlighters.HighlightJS
[![Build Status](https://dev.azure.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/_apis/build/status/Jering.Web.SyntaxHighlighters.HighlightJS-CI)](https://dev.azure.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/_build/latest?definitionId=4)
[![codecov](https://codecov.io/gh/JeringTech/Web.SyntaxHighlighters.HighlightJS/branch/master/graph/badge.svg)](https://codecov.io/gh/JeringTech/Web.SyntaxHighlighters.HighlightJS)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://github.com/Pkcs11Interop/Pkcs11Interop/blob/master/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/vpre/Jering.Web.SyntaxHighlighters.HighlightJS.svg?label=nuget)](https://www.nuget.org/packages/Jering.Web.SyntaxHighlighters.HighlightJS/)

## Table of Contents
[Overview](#overview)  
[Target Frameworks](#target-frameworks)  
[Prerequisites](#prerequisites)  
[Installation](#installation)  
[Concepts](#concepts)  
[Usage](#usage)  
[API](#api)  
[Building](#building)  
[Related Projects](#related-projects)  
[Contributing](#contributing)  
[About](#about)

## Overview
Jering.Web.SyntaxHighlighters.HighlightJS enables you to perform syntax highlighting from C# projects using [HighlightJS](http://highlightjs.readthedocs.io/en/latest/index.html).
Here is an example usage of this library:

```csharp
string code = @"public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}";

// Highlight code
string result = await StaticHighlightJSService.HighlightAsync(code, "csharp");

string syntaxHighlightedCode = @"<span class=""hljs-function""><span class=""hljs-keyword"">public</span> <span class=""hljs-keyword"">string</span> <span class=""hljs-title"">ExampleFunction</span>(<span class=""hljs-params""><span class=""hljs-keyword"">string</span> arg</span>)</span>
{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">""dummyString""</span>;
}";

// result == syntax highlighted code
Assert.Equal(syntaxHighlightedCode, result);
```

## Target Frameworks
- .NET Standard 2.0
- .NET Framework 4.6.1
 
## Prerequisites
[NodeJS](https://nodejs.org/en/) must be installed and node.exe's directory must be added to the `Path` environment variable.

## Installation
Using Package Manager:
```
PM> Install-Package Jering.Web.SyntaxHighlighters.HighlightJS
```
Using .Net CLI:
```
> dotnet add package Jering.Web.SyntaxHighlighters.HighlightJS
```

## Concepts
### What is a Syntax Highlighter?
Syntax highlighters add markup to code to facilitate styling. For example, the following code:

```csharp
public string ExampleFunction(string arg)
{
    // Example comment
    return arg + "dummyString";
}
```

is transformed into the following markup by the syntax highlighter HighlightJS:

```html
<span class="hljs-function"><span class="hljs-keyword">public</span> <span class="hljs-keyword">string</span> <span class="hljs-title">ExampleFunction</span>(<span class="hljs-params"><span class="hljs-keyword">string</span> arg</span>)
</span>{
    <span class="hljs-comment">// Example comment</span>
    <span class="hljs-keyword">return</span> arg + <span class="hljs-string">"dummyString"</span>;
}
```

HighlightJS is a a javascript library, which is ideal since syntax highlighting is often done client-side. There are however, situations where syntax highlighting can't or shouldn't be done client-side, for example:
- When generating [AMP](https://www.ampproject.org/) pages, since AMP pages cannot run scripts.
- When page load time is critical.
- When page size is critical.

This library allows syntax highlighting to be done by .Net server-side applications and tools like static site generators.

## Usage
### Creating IHighlightJSService
This library uses depedency injection (DI) to facilitate extensibility and testability.
You can use any DI framework that has adapters for [Microsoft.Extensions.DependencyInjection](https://github.com/aspnet/DependencyInjection).
Here, we'll use the vanilla Microsoft.Extensions.DependencyInjection framework:
```csharp
var services = new ServiceCollection();
services.AddHighlightJS();
ServiceProvider serviceProvider = services.BuildServiceProvider();
IHighlightJSService highlightJSService = serviceProvider.GetRequiredService<IHighlightJSService>();
```

`IHighlightJSService` is a singleton service and `IHighlightJSService`'s members are thread safe.
Where possible, inject `IHighlightJSService` into your types or keep a reference to a shared `IHighlightJSService` instance. 
Try to avoid creating multiple `IHighlightJSService` instances, since each instance spawns a NodeJS process. 

When you're done, you can manually dispose of an `IHighlightJSService` instance by calling
```csharp
highlightJSService.Dispose();
```
or 
```csharp
serviceProvider.Dispose(); // Calls Dispose on objects it has instantiated that are disposable
```
`Dispose` kills the spawned NodeJS process.
Note that even if `Dispose` isn't called manually, the service that manages the NodeJS process, `INodeJSService` from [Jering.Javascript.NodeJS](https://github.com/JeringTech/Javascript.NodeJS), will kill the 
NodeJS process when the application shuts down - if the application shuts down gracefully. If the application does not shutdown gracefully, the NodeJS process will kill 
itself when it detects that its parent has been killed. 
Essentially, manually disposing of `IHighlightJSService` instances is not mandatory.

#### Static API
This library also provides a static API as an alternative. The `StaticHighlightJSService` type wraps an `IHighlightJSService` instance, exposing most of its [public members](#api) statically.
Whether you use the static API or the DI based API depends on your development needs. If you are already using DI, if you want to mock 
out syntax highlighting in your tests or if you want to overwrite services, use the DI based API. Otherwise,
use the static API. An example usage:

```csharp
string result = await StaticHighlightJSService.HighlightAsync(code, "csharp");
```

The following section on using `IHighlightJSService` applies to usage of `StaticHighlightJSService`.

### Using IHighlightJSService
Code can be highlighted using [`IHighlightJSService.HighlightAsync`](#ihighlightjsservice.highlightasync):
```csharp
string code = @"public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}";

string highlightedCode = await highlightJSService.HighlightAsync(code, "csharp");
```
The second parameter of `IHighlightJSService.HighlightAsync` must be a valid [HighlightJS language alias](http://highlightjs.readthedocs.io/en/latest/css-classes-reference.html#language-names-and-aliases).

## API
### IHighlightJSService.HighlightAsync
#### Signature
```csharp
Task<string> HighlightAsync(string code, string languageAlias, string classPrefix = "hljs-", CancellationToken cancellationToken = default)
```
#### Description
Highlights code of a specified language.
#### Parameters
- `code`
  - Type: `string`
  - Description: Code to highlight.
- `languageAlias`
  - Type: `string`
  - Description: A HighlightJS language alias. Visit http://highlightjs.readthedocs.io/en/latest/css-classes-reference.html#language-names-and-aliases for the list of valid language aliases.
- `classPrefix`
  - Type: `string`
  - Description: If not null or whitespace, this string will be appended to HighlightJS classes. Defaults to `hljs-`.
- `cancellationToken`
  - Type: `CancellationToken`
  - Description: The cancellation token for the asynchronous operation.
#### Returns
Highlighted code.
#### Exceptions
- `ArgumentNullException`
  - Thrown if `code` is null.
- `ArgumentException`
  - Thrown if `languageAlias` is not a valid HighlightJS language alias.
- `InvocationException`
  - Thrown if a NodeJS error occurs.
- `ObjectDisposedException`
  - Thrown if this instance has been disposed or if an attempt is made to use one of its dependencies that has been disposed.
- `OperationCanceledException`
  - Thrown if `cancellationToken` is cancelled.
#### Example
```csharp
string code = @"public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}";

string highlightedCode = await highlightJSService.HighlightAsync(code, "csharp");
```
### IHighlightJSService.IsValidLanguageAliasAsync
#### Signature
```csharp
Task<bool> IsValidLanguageAliasAsync(string languageAlias, CancellationToken cancellationToken = default)
```
#### Description
Determines whether a language alias is valid.
#### Parameters
- `languageAlias`
  - Type: `string`
  - Description: Language alias to validate. Visit http://highlightjs.readthedocs.io/en/latest/css-classes-reference.html#language-names-and-aliases for the list of valid language aliases.
#### Returns
`true` if `languageAlias` is a valid HighlightJS language alias. Otherwise, `false`.
#### Exceptions
- `InvocationException`
  - Thrown if a NodeJS error occurs.
- `ObjectDisposedException`
  - Thrown if this instance has been disposed or if an attempt is made to use one of its dependencies that has been disposed.
- `OperationCanceledException`
  - Thrown if `cancellationToken` is cancelled.
#### Example
```csharp
bool isValid = await highlightJSService.IsValidLanguageAliasAsync("csharp");
```

### API Notes
If you've used the javascript HighlightJS library before, you might have noticed that some of its features have been omitted in this
wrapper. The following are the reasons for their omittance:

#### ignore_illegals
If [ignore_illegals](http://highlightjs.readthedocs.io/en/latest/api.html#highlight-name-value-ignore-illegals-continuation) is false, the javascript HighlightJS library 
throws an error when invalid syntax is detected. 
This feature can be inaccurate because language definitions aren't always up to date.

#### Automatic Language Detection
The javascript HighlightJS library has an [automatic language detection](http://highlightjs.readthedocs.io/en/latest/api.html#highlightauto-value-languagesubset) feature. It works by 
highlighting code using every language definition, then ranking languages based on 
the number of matches for each language definition (a language definition is essentially a set of regex expressions). 
This feature can be inaccurate, especially for short snippets.

#### Continuation
The javascript HighlightJS library has a [continuation](http://highlightjs.readthedocs.io/en/latest/api.html#highlight-name-value-ignore-illegals-continuation) feature. Essentially,
it returns the context of every highlight call, allowing subsequent calls to continue highlighting based on the context of an earlier call.
Making multiple highlight calls with the continuation feature is equivalent to concatenating the code and making a single highlight call. For this wrapper,
a single highlight call is far more performant since it minimizes time spent on object marshalling and inter-process communication.

## Building
This project can be built using Visual Studio 2017.

## Related Jering Projects
#### Similar Projects
[Jering.Web.SyntaxHighlighters.Prism](https://github.com/JeringTech/Web.SyntaxHighlighters.Prism) - Use the Syntax Highlighter, Prism, from C#.
#### Projects Using this Library
[Jering.Markdig.Extensions.FlexiBlocks](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks) - A Collection of Flexible Markdig Extensions.
#### Projects this Library Uses
[Jering.Javascript.NodeJS](https://github.com/JeringTech/Javascript.NodeJS) - Invoke Javascript in NodeJS, from C#.

## Contributing
Contributions are welcome!  

## About
Follow [@JeringTech](https://twitter.com/JeringTech) for updates and more.