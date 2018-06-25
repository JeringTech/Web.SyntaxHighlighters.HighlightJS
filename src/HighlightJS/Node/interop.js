var hljs = require('highlight.js');

var hljsDefaultOptions = {
    classPrefix: 'hljs-',
    tabReplace: null,
    useBR: false,
    languages: undefined
}
module.exports = {
    highlight: function (callback, languageAlias, code, ignoreIllegals, tabReplace, useBR, classPrefix) {
        debugger;

        // Since HighlightJSService has to be thread safe, an atomic way to configure options for a specific highlight call is required
        var configure = classPrefix !== hljsDefaultOptions.classPrefix ||
            tabReplace !== hljsDefaultOptions.tabReplace ||
            useBR !== hljsDefaultOptions.useBR;
        if (configure) {
            hljs.configure({
                classPrefix: classPrefix,
                tabReplace: tabReplace,
                useBR: useBR
            });
        }

        // Most of the values in the result object aren't useful to the hljs wrapper. 
        // 
        // highlightResult.top facilitates highlighting of additional code. It is the context that 
        // the current code was highlighted in and is meant to be used as the continuation parameter in hljs.highlight.
        // Continuation isn't useful to the hljs wrapper, since it is far more efficient to concatenate code and highlight 
        // all of it at once. Also, highlightResult.top has a circular structure, making it tricky to serialize and deserialize as JSON.
        //
        // highlightResult.language and highlightResult.relevance are only relevant to highlightAuto.
        var highlightResult = hljs.highlight(languageAlias, code, ignoreIllegals);

        // Reset hljs settings
        if (configure) {
            hljs.configure(hljsDefaultOptions);
        }

        callback(null /* errors */, highlightResult.value);
    },
    highlightAuto: function (callback, code, languageSubset, tabReplace, useBR, classPrefix) {
        debugger;

        // Since HighlightJSService has to be thread safe, an atomic way to configure options for a specific highlight call is required
        var configure = classPrefix !== hljsDefaultOptions.classPrefix ||
            tabReplace !== hljsDefaultOptions.tabReplace ||
            useBR !== hljsDefaultOptions.useBR;
        if (configure) {
            hljs.configure({
                classPrefix: classPrefix,
                tabReplace: tabReplace,
                useBR: useBR
            });
        }

        var result = hljs.highlightAuto(code, languageSubset); // TODO ensure that language subset can be null

        // Reset hljs settings
        if (configure) {
            hljs.configure(hljsDefaultOptions);
        }

        callback(null /* errors */, result);
    },
    getLanguageNames: function (callback) {
        var result = hljs.getLanguageNames();

        callback(null /* errors */, result);
    }
};