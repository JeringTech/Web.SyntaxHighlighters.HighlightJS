# Changelog
This project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html). Refer to 
[The Semantic Versioning Lifecycle](https://www.jeremytcd.com/articles/the-semantic-versioning-lifecycle)
for an overview of semantic versioning.

## [Unreleased](https://github.com/JeremyTCD/Web.SyntaxHighlighters.HighlightJS/compare/2.2.0...HEAD)

## [2.2.0](https://github.com/JeremyTCD/Web.SyntaxHighlighters.HighlightJS/compare/2.1.0...2.2.0) - Oct 4, 2018
## Changes
- Bumped `Microsoft.Extensions.DependencyInjection` version.

## [2.1.0](https://github.com/JeremyTCD/Web.SyntaxHighlighters.HighlightJS/compare/2.0.0...2.1.0) - Aug 9, 2018
## Changes
- Bumped `Jering.Javascript.NodeJS` version.

## [2.0.0](https://github.com/JeremyTCD/Web.SyntaxHighlighters.HighlightJS/compare/1.1.0...2.0.0) - Aug 6, 2018
### Changes
- Renamed project to `Jering.Web.SyntaxHighlighters.HighlightJS` for consistency with other `Jering` packages. Using statements must be updated to reference types from the
namespace `Jering.Web.SyntaxHighlighters.HighlightJS` instead of `Jering.WebUtils.SyntaxHighlighters.HighlightJS`.
- Added .NET Standard 1.3 as a target framework.

## [1.1.0](https://github.com/JeremyTCD/Web.SyntaxHighlighters.HighlightJS/compare/1.0.1...1.1.0) - Jul 25, 2018
### Changes
- Replaced [Microsoft.AspNetCore.NodeServices](https://github.com/aspnet/JavaScriptServices/tree/master/src/Microsoft.AspNetCore.NodeServices) with 
  [JavascriptUtils.NodeJS](https://github.com/JeremyTCD/JavascriptUtils.NodeJS) for IPC with Node.js.
- Renamed assembly to Jering.WebUtils.SyntaxHighlighters.HighlightJS.
### Additions
- Added XML comments in Nuget package.

## [1.0.1](https://github.com/JeremyTCD/Web.SyntaxHighlighters.HighlightJS/compare/1.0.0...1.0.1) - Jul 3, 2018
### Fixes
- Fixed dev dependencies not being excluded from Nuget package.

## 1.0.0 - Jun 29, 2018
Initial release.