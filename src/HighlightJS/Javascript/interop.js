var hljs = require('highlight.js/lib/core.js');

// Monkey patch registerLanguage to obtain complete list of aliases
var languageAliases = [];
var registerLanguage = hljs.registerLanguage;
hljs.registerLanguage = (name, languageFactory) => {
    languageAliases.push(name);
    var language = languageFactory(hljs);
    if (language.aliases) {
        language.
            aliases.
            forEach((alias) => languageAliases.push(alias));
    }
    registerLanguage(name, languageFactory);
};

// Registers languages
require('highlight.js/lib/index.js');

var hljsOptions = { classPrefix: 'hljs-' };

module.exports = {
    highlight: function (callback, code, languageAlias, classPrefix) {
        // Since HighlightJSService has to be thread safe, this atomic means of configure options for a specific highlight call is required
        if (classPrefix !== hljsOptions.classPrefix) {
            hljsOptions.classPrefix = classPrefix;
            hljs.configure(hljsOptions);
        }

        // Most of the values in the result object aren't useful to the hljs wrapper. 
        // 
        // highlightResult.top facilitates highlighting of additional code. It is the context that 
        // the current code was highlighted in and is meant to be used as the continuation parameter in hljs.highlight.
        // Continuation isn't useful to the hljs wrapper, since it is far more efficient to concatenate code and highlight 
        // all of it at once. Also, highlightResult.top has a circular structure, making it tricky to serialize and deserialize as JSON.
        //
        // highlightResult.language and highlightResult.relevance are only relevant to highlightAuto.
        var highlightResult = hljs.highlight(languageAlias, code, true);

        callback(null /* errors */, highlightResult.value);
    },
    getAliases: function (callback) {
        callback(null /* errors */, languageAliases);
    }
};
