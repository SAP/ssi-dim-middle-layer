# Summary

The main process worker project is the `Processes.Worker` which runs all the processes. It therefor looks up `process_steps` in status `TODO` and their respective `processes` and executes those.

## Processes

The process worker supports the following processes:

- [CreateWallet](../processes/01.%20create_wallet.md) - handles the creation of wallets
- [CreateTechnicalUser](../processes/02.%20create_technical_user.md) - handles the creation of technical user
- [DeleteTechnicalUser](../processes/03.%20delete_technical_user.md) - handles the deletion of technical user

## Retriggering

The process has a logic to retrigger failing steps. For this a retrigger step is created which can be triggered via an api call to retrigger the step. This logic is implemented separately for each process. In general the retriggering of a step is possible if for example external services are not available. The retrigger logic for each process can be found in the process file.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer
