# DeepMatch [![Build Status](https://travis-ci.org/GrafGenerator/DeepMatch.svg?branch=develop)](https://travis-ci.org/GrafGenerator/DeepMatch)
The C# library for pattern matching.

**Note:** this is ***very early development***, and a subject for major changes in the future.

## Release notes
### v0.0.3
- Fixed issue when heads array in result function is polluted with extra values, if before fully matched block was block(s), that matched partially. For example, blocks (1, 2, 4) and (1, 2, 3) on sequence (1, 2, 3, ...).

### v0.0.2
- Fixed issue when match block is not match anything because of extra enumerator movement when using tail func.

### v0.0.1
- Basic working implementation, recursive sum sample in tests.
