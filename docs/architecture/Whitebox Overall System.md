# Whitebox Overall System

## Summary

In the following image you see the overall system overview of the SSI DIM Middle Layer

```mermaid
flowchart LR

    C(Customer)
    ING(Ingress)
    DS(DIM Service)
    PW(Process Worker)
    P(Portal)
    SD(SAP DIM)
    PHD[("Postgres Database \n \n (Base data created with \n application seeding)")]

    subgraph SSI DIM Middle Layer Product   
        ING
        PHD
        DS
        PW
    end

    subgraph External Systems
        P
        SD
    end

    C-->|"Authentication & Authorization Data \n (Using JWT)"|ING
    ING-->|"Forward Request"|DS
    PW-->|"Read, Write Wallet & Technical User Data"|PHD
    PW-->|"Callback wallet & technical user information"|P
    DS-->|"Create wallets, \n manage technical users"|PHD

```

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer
