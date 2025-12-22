# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.26] - 2025-12-22

### Changed
- Updated solution structure by removing KPS.Console project and adding KPS.Console.Net6 and KPS.Console.Net8 projects
- Adjusted package references for improved .NET version compatibility

## [1.0.25] - 2025-12-20

### Changed
- Documentation improvements for KpsClientBuilder usage examples

## [1.0.24] - 2025-12-20

### Changed
- Updated README and console examples to clarify usage of KpsClientBuilder with minimal and advanced configurations
- Removed duplicate console output for successful person validation

## [1.0.23] - 2025-12-19

### Added
- Introduced PersonInfo model for detailed person data representation

### Changed
- Removed deprecated kpsclient subproject
- Enhanced XML operations for improved consistency with Go implementation
- Improved XML formatting and alignment with Go implementation in SignSoapEnvelopeOperation
- Updated QueryResult to include PersonInfo property
- Enhanced XML response parsing for improved data handling

## [1.0.21] - 2025-12-19

### Changed
- Enhanced CreateSoapEnvelopeOperation and SignSoapEnvelopeOperation for improved XML formatting
- Improved consistency with Go implementation

## [1.0.20] - 2025-12-18

### Changed
- Enhanced SignSoapEnvelopeOperation by explicitly declaring XML namespaces
- Improved consistency with Go implementation

## [1.0.19] - 2025-12-18

### Changed
- Improved signature generation process in SignSoapEnvelopeOperation
- Ensured signature calculation occurs before appending SignedInfo to the parent element

## [1.0.18] - 2025-12-18

### Changed
- Updated CreateSoapEnvelope and CreateWsTrustRequest methods to remove KpsOptions dependency
- Enhanced XML generation consistency

## [1.0.17] - 2025-12-18

### Changed
- Updated CreateSoapEnvelopeOperation and CreateWsTrustRequestOperation to utilize KpsOptions
- Improved XML generation consistency

## [1.0.16] - 2025-12-18

### Changed
- Streamlined SignSoapEnvelopeOperation by modularizing XML node creation
- Improved signature generation process

## [1.0.15] - 2025-12-18

### Changed
- Refactored CreateSoapEnvelopeOperation and CreateWsTrustRequestOperation to use constants for namespaces
- Improved XML generation consistency

## [1.0.14] - 2025-12-18

### Changed
- Enhanced SignSoapEnvelopeOperation with improved error handling
- Improved XML signature generation matching Go implementation

## [1.0.13] - 2025-12-17

### Changed
- Modified SignSoapEnvelopeOperation to clear Security node children without removing attributes
- Ensured SOAP envelope integrity

## [1.0.12] - 2025-12-17

### Changed
- Refactored ParseSoapResponseOperation for namespace-agnostic SOAP fault and response handling

## [1.0.11] - 2025-12-17

### Changed
- Refactored SignSoapEnvelopeOperation to use MemoryStream for output handling

## [1.0.10] - 2025-12-17

### Changed
- Standardized code formatting in CreateSoapEnvelopeOperation and SignSoapEnvelopeOperation
- Improved readability

## [1.0.9] - 2025-12-17

### Changed
- Updated timestamp formatting in CreateWsTrustRequestOperation and SignSoapEnvelopeOperation
- Now uses CultureInfo.InvariantCulture for consistency

## [1.0.8] - 2025-12-16

### Added
- Added TokenXml property to KpsOptions

### Changed
- Updated related logic in SoapService and StsService for improved error handling
- Enhanced STS artifact extraction

## [1.0.7] - 2025-10-07

### Changed
- Replaced integer result codes with enum in QueryResult
- Updated related logic in ParseSoapResponseOperation and tests

## [1.0.6] - 2025-10-07

### Changed
- Updated target framework to net8.0 in KPS.Console project
- Enhanced citizen verification logic with error handling and detailed response messages

## [1.0.5] - 2025-10-07

### Changed
- Simplified KPS service implementations by removing logger dependencies
- Utilized factory pattern for XML operations

### Added
- Added SigningKey and AssertionId properties to KpsOptions

## [1.0.4] - 2025-10-07

### Changed
- Updated KPS client endpoints
- Removed obsolete interfaces

## [1.0.3] - 2025-10-07

### Changed
- Renamed QueryRequest to CitizenVerificationRequest
- Updated related methods in KPS.Core

## [1.0.2] - 2025-10-06

### Changed
- Simplified target frameworks in KPS.Core project file
- Support for multiple .NET versions (net6.0, net8.0, net9.0, net10.0)

## [1.0.1] - 2025-10-06

### Changed
- Renamed DoQueryAsync to QueryAsync for consistency
- Updated README documentation

## [1.0.0] - 2025-10-06

### Added
- Initial release of KPS.Core
- .NET client library for Turkey's Population and Citizenship Affairs (KPS) v2 services
- WS-Trust authentication support
- HMAC-SHA1 signed SOAP requests
- KpsClientBuilder for fluent client configuration
- CitizenVerificationRequest model for identity verification
- QueryResult and PersonInfo models for response handling
- Support for .NET 6.0, 8.0, 9.0, and 10.0
- Comprehensive test suite
- MIT License

[1.0.26]: https://github.com/onurkanbakirci/KPS/compare/v1.0.25...v1.0.26
[1.0.25]: https://github.com/onurkanbakirci/KPS/compare/v1.0.24...v1.0.25
[1.0.24]: https://github.com/onurkanbakirci/KPS/compare/v1.0.23...v1.0.24
[1.0.23]: https://github.com/onurkanbakirci/KPS/compare/v1.0.21...v1.0.23
[1.0.21]: https://github.com/onurkanbakirci/KPS/compare/v1.0.20...v1.0.21
[1.0.20]: https://github.com/onurkanbakirci/KPS/compare/v1.0.19...v1.0.20
[1.0.19]: https://github.com/onurkanbakirci/KPS/compare/v1.0.18...v1.0.19
[1.0.18]: https://github.com/onurkanbakirci/KPS/compare/v1.0.17...v1.0.18
[1.0.17]: https://github.com/onurkanbakirci/KPS/compare/v1.0.16...v1.0.17
[1.0.16]: https://github.com/onurkanbakirci/KPS/compare/v1.0.15...v1.0.16
[1.0.15]: https://github.com/onurkanbakirci/KPS/compare/v1.0.14...v1.0.15
[1.0.14]: https://github.com/onurkanbakirci/KPS/compare/v1.0.13...v1.0.14
[1.0.13]: https://github.com/onurkanbakirci/KPS/compare/v1.0.12...v1.0.13
[1.0.12]: https://github.com/onurkanbakirci/KPS/compare/v1.0.11...v1.0.12
[1.0.11]: https://github.com/onurkanbakirci/KPS/compare/v1.0.10...v1.0.11
[1.0.10]: https://github.com/onurkanbakirci/KPS/compare/v1.0.9...v1.0.10
[1.0.9]: https://github.com/onurkanbakirci/KPS/compare/v1.0.8...v1.0.9
[1.0.8]: https://github.com/onurkanbakirci/KPS/compare/v1.0.7...v1.0.8
[1.0.7]: https://github.com/onurkanbakirci/KPS/compare/v1.0.6...v1.0.7
[1.0.6]: https://github.com/onurkanbakirci/KPS/compare/v1.0.5...v1.0.6
[1.0.5]: https://github.com/onurkanbakirci/KPS/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/onurkanbakirci/KPS/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/onurkanbakirci/KPS/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/onurkanbakirci/KPS/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/onurkanbakirci/KPS/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/onurkanbakirci/KPS/releases/tag/v1.0.0

