# Operational concepts

## SSI DIM Middle Layer Services

### Configuration

The SSI DIM Middle Layer services can be configured using two methods:

### appsettings.json

If you build the SSI DIM Middle Layer, you can modify the appsettings.json for each backend service, to individually configure to a certain extend. This file contains all possible config entries for the application.

### Helm Chart

The most relevant config properties are exposed as environment variables and must be set in the Helm chart so the application can run at all. Check the SSI DIM Middle Layer Helm chart in Git for all available variables.

### DB Migration File

Static Data migration files provide a certain configuration possibility by adding or deleting static data records before the deployment. Be aware that touching static data files will always impact the application business process. It is suggested to always test the application with the planned changes carefully in INT before releasing to a productive env.

## Monitoring

Currently all backend services write log entries as structural data in json format. These logs can easily be monitored. There are several options to provide a stable monitoring solution, one of them is to setup loki and grafana. In this solution loki is used as a datasource and custom dashboards can be setup in grafana to monitor the services. Some general Properties to query with grafana are:

- StatusCode - contains the status code of the response
- Elapsed - contains the time a endpoint took to response in milliseconds
- RenderedMessage - contains the log message with possible errors

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer
