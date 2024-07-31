# Architecture Constraints Documentation

## Overview

The following document outlines the architecture constraints for the SSI Dim Middle Layer App. This App serves as a central point for wallet as well as technical user creation. The constraints outlined in this document are intended to guide the development and deployment of the system to ensure it meets the specified requirements and adheres to the defined standards.

## General Constraints

### System Purpose

- **Communication**: The App facilitates communication with wallets and technical users.
- **No User Interface (UI)**: The current development plan does not include the implementation of a user interface.

### Deployment

- **Run Anywhere**: The system is designed to be containerized and deployable as a Docker image. This ensures it can run on various platforms, including cloud environments, on-premises infrastructure, or locally.
- **Platform-Independent**: The application is platform-independent, capable of running on Kubernetes or similar orchestration platforms.

## Developer Constraints

### Open Source Software

- **Apache License 2.0**: The Apache License 2.0 is selected as the approved license to respect and guarantee intellectual property rights.

### Development Standards

- **Coding Guidelines**: Defined coding guidelines for frontend (FE) and backend (BE) development must be followed for all portal-related developments.
- **Consistency Enforcement**: Code analysis tools, linters, and code coverage metrics are used to enforce coding standards and maintain a consistent style. These standards are enforced through the Continuous Integration (CI) process to prevent the merging of non-compliant code.

## Code Analysis and Security

To ensure code quality and security, the following analyses and checks are performed during standard reviews:

### Code Quality Checks

- **Code Linting**: Tools to enforce coding style and detect syntax errors.
- **Code Coverage**: Metrics to ensure a sufficient percentage of the codebase is covered by automated tests.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer