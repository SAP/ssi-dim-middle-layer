# Dev flow with deployment to dev environment

```mermaid
flowchart LR
    subgraph local
    D(Developer)
    end
    subgraph eclipse-tractusx
        direction LR
        D -- PR* to main*--> SDML(ssi-dim-middle-layer**)
        click SCI "https://github.com/eclipse-tractusx/ssi-dim-middle-layer"
    end
    subgraph Argo CD - sync to k8s cluster
    SCI -- auto-sync --> A(Argo CD dev)
    end
```

Note\* Every pull request (PR) requires at least one approving review by a committer

Note\*\* Unit tests and code analysis checks run at pull request

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer
