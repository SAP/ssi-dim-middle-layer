# Identity Access Management (IAM)

## Role concept

The endpoints of the SSI DIM Middle Layer can only be used with an authenticated and authorized user. To be able to authorize the (technical) user, he needs to have the configured audience which is set in the appsettings under `JWTBEAREROPTIONS__TOKENVALIDATIONPARAMETERS__VALIDAUDIENCE`. The configured audience **must** be added as a client in the IAM system.

The following roles need to exist in the client that is configured as the valid audience and need to be assigned to the (technical) user:

| Role | Endpoint |
|-----------------------|------------------------------------------------------|
|     setup_wallet      | POST: api/dim/setup-dim & POST: api/dim/setup-issuer |
|    view_status_list   | GET: api/dim/status-list/{bpn}                       |
|   create_status_list  | POST: api/dim/status-list/{bpn}                      |
| create_technical_user | POST: api/dim/technical-user/{bpn}                   |
| delete_technical_user | POST: api/dim/technical-user/{bpn}/delete            |

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer
