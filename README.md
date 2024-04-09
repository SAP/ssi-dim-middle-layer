[![REUSE status](https://api.reuse.software/badge/github.com/SAP/ssi-dim-middle-layer)](https://api.reuse.software/info/github.com/SAP/ssi-dim-middle-layer)

# ssi-dim-middle-layer

## About this project

This repository contains the backend code for the Integration layer between DIM solution and Tractus-X Portal written in C#. It's used to create a wallet and configure all needed information.

For **installation** details and further information, please refer to the chart specific [README](./charts/dim/README.md).

## Requirements and Setup

Install the [.NET 8.0 SDK](https://www.microsoft.com/net/download).

Run the following command from the CLI:

```console
dotnet build src
```

Make sure the necessary config is added to the settings of the service you want to run.
Run the following command from the CLI in the directory of the service you want to run:

```console
dotnet run
```

## Support, Feedback, Contributing

This project is open to feature requests/suggestions, bug reports etc. via [GitHub issues](https://github.com/SAP/ssi-dim-middle-layer/issues). Contribution and feedback are encouraged and always welcome. For more information about how to contribute, the project structure, as well as additional contribution information, see our [Contribution Guidelines](CONTRIBUTING.md).

## Security / Disclosure
If you find any bug that may be a security problem, please follow our instructions at [in our security policy](https://github.com/SAP/ssi-dim-middle-layer/security/policy) on how to report it. Please do not create GitHub issues for security-related doubts or problems.

## Code of Conduct

We as members, contributors, and leaders pledge to make participation in our community a harassment-free experience for everyone. By participating in this project, you agree to abide by its [Code of Conduct](https://github.com/SAP/.github/blob/main/CODE_OF_CONDUCT.md) at all times.

## Licensing

Copyright 2024 SAP SE or an SAP affiliate company and ssi-dim-middle-layer contributors. Please see our [LICENSE](LICENSE) for copyright and license information. Detailed information including third-party components and their licensing/copyright information is available [via the REUSE tool](https://api.reuse.software/info/github.com/SAP/ssi-dim-middle-layer).
