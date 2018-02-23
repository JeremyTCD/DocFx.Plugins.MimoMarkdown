# Mimo Markdown
**This ReadMe is incomplete (work in progress)**

Custom markdown for [Mimo](https://github.com/JeremyTCD/Mimo).

## Things to Do
- Add container, injection.
- Write unit tests.
- Create inline rules for include-* tokens. Register using `DfmEngineBuilder.InlineRules`.
- Add sanitize option for include-markdown, consider security issues with include-code.
- Create shared interface for Range and Region so that they can both be specified in the same inumerable and thus used together.
- IncludeCodeOptions.Title's <language> token should be inferred from file extension if possible.
- Add support for more protocols in FileRetrievalService.

