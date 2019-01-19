# Changelog
This project uses [semantic versioning](http://semver.org/spec/v2.0.0.html). Refer to 
*[Semantic Versioning in Practice](https://www.jering.tech/articles/semantic-versioning-in-practice)*
for an overview of semantic versioning.

## [Unreleased](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/3.1.0...HEAD)

## [3.1.0](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/3.0.1...3.1.0) - Dec 3, 2018
### Additions
- Added `StaticHighlightJSService.DisposeServiceProvider`.
### Fixes
- `StaticHighlightJSService.Invoke*` methods are now thread-safe.

## [3.0.1](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/3.0.0...3.0.1) - Nov 30, 2018
### Changes
- Changed project URL (used by NuGet.org) from `jering.tech/utilities/web.syntaxhighlighters.highlightjs` to `jering.tech/utilities/jering.web.syntaxhighlighters.highlightjs` for consistency with other Jering projects.

## [3.0.0](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/2.2.0...3.0.0) - Nov 29, 2018
### Additions
- Added `StaticHighlightJSService`.
- Added `Dispose(bool disposed)` and a finalizer to `HighlightJSService`.
### Changes
- **Breaking**: `IHighlightJSService.HighlightAsync` and `IHighlightJSService.IsValidLanguageAliasAsync` now take
a `CancellationToken` as an optional argument.
- Improved XML comments.
- Bumped `Jering.Javascript.NodeJS` to 4.0.3.
- Updated Nuget package metadata.

## [2.2.0](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/2.1.0...2.2.0) - Oct 4, 2018
### Changes
- Bumped `Microsoft.Extensions.DependencyInjection` version.

## [2.2.0](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/2.1.0...2.2.0) - Oct 4, 2018
### Changes
- Bumped `Microsoft.Extensions.DependencyInjection` version.

## [2.1.0](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/2.0.0...2.1.0) - Aug 9, 2018
### Changes
- Bumped `Jering.Javascript.NodeJS` version.

## [2.0.0](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/1.1.0...2.0.0) - Aug 6, 2018
### Changes
- Renamed project to `Jering.Web.SyntaxHighlighters.HighlightJS` for consistency with other `Jering` packages. Using statements must be updated to reference types from the
namespace `Jering.Web.SyntaxHighlighters.HighlightJS` instead of `Jering.WebUtils.SyntaxHighlighters.HighlightJS`.
- Added .NET Standard 1.3 as a target framework.

## [1.1.0](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/1.0.1...1.1.0) - Jul 25, 2018
### Additions
- Added XML comments in Nuget package.
### Changes
- Replaced [Microsoft.AspNetCore.NodeServices](https://github.com/aspnet/JavaScriptServices/tree/master/src/Microsoft.AspNetCore.NodeServices) with 
  [JavascriptUtils.NodeJS](https://github.com/JeringTech/JavascriptUtils.NodeJS) for IPC with Node.js.
- Renamed assembly to Jering.WebUtils.SyntaxHighlighters.HighlightJS.

## [1.0.1](https://github.com/JeringTech/Web.SyntaxHighlighters.HighlightJS/compare/1.0.0...1.0.1) - Jul 3, 2018
### Fixes
- Fixed dev dependencies not being excluded from Nuget package.

## 1.0.0 - Jun 29, 2018
Initial release.
