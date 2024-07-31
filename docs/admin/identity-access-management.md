# Identity Access Management

## Role concept

The endpoints of the SSI Dim Middle Layer can only be used with an authenticated user. To be able to authorize the user, he needs to have the configured Audience which is set in the appsetting under `JWTBEAREROPTIONS__TOKENVALIDATIONPARAMETERS__VALIDAUDIENCE. The configured Audience **must** be added as a client in the idp.

The following roles do exist and need to be exist in the Client that is configured as the valid audience:

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
