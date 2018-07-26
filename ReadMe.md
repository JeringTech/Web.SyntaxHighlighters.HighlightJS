# Jering.WebUtils.SyntaxHighlighters.HighlightJS
[![Build status](https://ci.appveyor.com/api/projects/status/wawhrh1nvy5fae2s?svg=true)](https://ci.appveyor.com/project/JeremyTCD/webutils-syntaxhighlighters-highlightjs)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://github.com/Pkcs11Interop/Pkcs11Interop/blob/master/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/vpre/Jering.WebUtils.SyntaxHighlighters.HighlightJS.svg?label=nuget)](https://www.nuget.org/packages/Jering.WebUtils.SyntaxHighlighters.HighlightJS/)
<!-- TODO tests badge, this service should work - https://github.com/monkey3310/appveyor-shields-badges/blob/master/README.md -->

Perform Syntax Highlighting in .Net Applications Using the Javascript Library, [HighlightJS](http://highlightjs.readthedocs.io/en/latest/index.html). 

## Table of Contents
[Overview](#overview)  
[Prerequisites](#prerequisites)  
[Installation](#installation)  
[Usage](#usage)  
[Building](#building)  
[Related Projects](#related-projects)  
[Contributing](#contributing)  
[About](#about)

## Overview
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

WebUtils.SyntaxHighlighters.HighlightJS allows syntax highlighting to be done by .Net server-side applications and tools like static site generators.

## Prerequisites
[Node.js](https://nodejs.org/en/) must be installed and node.exe's directory must be added to the `Path` environment variable.

## Installation
Using Package Manager:
```
PM> Install-Package Jering.WebUtils.SyntaxHighlighters.HighlightJS
```
Using .Net CLI:
```
> dotnet add package Jering.WebUtils.SyntaxHighlighters.HighlightJS
```

## Usage
### Creating `IHighlightJSService` in ASP.NET Apps
ASP.NET has a built in dependency injection (DI) system. This system can handle instantiation and disposal of `IHighlightJSService` instances.
Call `AddHighlightJS` in `Startup.ConfigureServices` to register a service for `IHighlightJSService`:
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHighlightJS();
}
```
You can then inject `IHighlightJSService` into controllers:
```csharp
public MyController(IHighlightJSService highlightJSService)
{
    _highlightJSService = highlightJSService;
}
```

### Creating `IHighlightJSService` in non-ASP.NET Apps
In non-ASP.NET projects, you'll have to create your own DI container. For example, using [Microsoft.Extensions.DependencyInjection](https://github.com/aspnet/DependencyInjection):
```csharp
var services = new ServiceCollection();
services.AddHighlightJS();
ServiceProvider serviceProvider = services.BuildServiceProvider();
IHighlightJSService highlightJSService = serviceProvider.GetRequiredService<IHighlightJSService>();
```
`IHighlightJSService` is a singleton service and `IHighlightJSService`'s members are thread safe.
Where possible, inject `IHighlightJSService` into your types or keep a reference to a shared `IHighlightJSService` instance. 
Try to avoid creating multiple `IHighlightJSService` instances, since each instance spawns a Node.js process. 

When you're done, you can manually dispose of an `IHighlightJSService` instance by calling
```csharp
highlightJSService.Dispose();
```
or 
```csharp
serviceProvider.Dispose(); // Calls Dispose on objects it has instantiated that are disposable
```
`Dispose` kills the spawned Node.js process.
Note that even if `Dispose` isn't called manually, the service that manages the Node.js process, `INodeJSService` from [Jering.JavascriptUtils.NodeJS](https://github.com/JeremyTCD/JavascriptUtils.NodeJS), will kill the 
Node.js process when the application shuts down - if the application shuts down gracefully. If the application does not shutdown gracefully, the Node.js process will kill 
itself when it detects that its parent has been killed. 
Essentially, manually disposing of `IHighlightJSService` instances is not mandatory.

### API
#### IHighlightJSService.HighlightAsync
##### Signature
```csharp
Task<string> HighlightAsync(string code, string languageAlias, string classPrefix = "hljs-")
```
##### Description
Highlights code of a specified language.
##### Parameters
- `code`
  - Type: `string`
  - Description: Code to highlight.
- `languageAlias`
  - Type: `string`
  - Description: A HighlightJS language alias. Visit http://highlightjs.readthedocs.io/en/latest/css-classes-reference.html#language-names-and-aliases for the list of valid language aliases.
- `classPrefix`
  - Type: `string`
  - Description: If not null or whitespace, this string will be appended to HighlightJS classes. Defaults to `hljs-`.
##### Returns
Highlighted code.
##### Exceptions
- `ArgumentNullException`
  - Thrown if `code` is null.
- `ArgumentException`
  - Thrown if `languageAlias` is not a valid HighlightJS language alias.
- `InvocationException`
  - Thrown if a NodeJS error occurs.
##### Example
```csharp
string code = @"public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}";

string highlightedCode = await highlightJSService.HighlightAsync(code, "csharp");
```
#### IHighlightJSService.IsValidLanguageAliasAsync
##### Signature
```csharp
ValueTask<bool> IsValidLanguageAliasAsync(string languageAlias)
```
##### Description
Determines whether a language alias is valid.
##### Parameters
- `languageAlias`
  - Type: `string`
  - Description: Language alias to validate. Visit http://highlightjs.readthedocs.io/en/latest/css-classes-reference.html#language-names-and-aliases for the list of valid language aliases.
##### Returns
`true` if `languageAlias` is a valid HighlightJS language alias. Otherwise, `false`.
##### Exceptions
- `InvocationException`
  - Thrown if a NodeJS error occurs.
##### Example
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

## Related Projects
#### Similar Projects
[Jering.WebUtils.SyntaxHighlighters.Prism](https://github.com/JeremyTCD/WebUtils.SyntaxHighlighters.Prism) - 
A C# Wrapper for the Syntax Highlighter, Prism.
#### Projects Using this Library
[Jering.Markdig.Extensions.FlexiBlocks](https://github.com/JeremyTCD/Markdig.Extensions.FlexiBlocks) - A Collection of Flexible Markdig Extensions.
#### Projects this Library Uses
[Jering.JavascriptUtils.NodeJS](https://github.com/JeremyTCD/JavascriptUtils.NodeJS) - A Tool for Invoking Javascript from C#.

## Contributing
Contributions are welcome!  

## About
Follow [@JeremyTCD](https://twitter.com/JeremyTCD) for updates and more.