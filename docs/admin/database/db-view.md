# Database View

- [Database View](#database-view)
  - [Database Overview](#database-overview)
  - [Database Structure](#database-structure)
    - [PROCESS\_STEP\_STATUSES](#process_step_statuses)
      - [Possible Values](#possible-values)
    - [PROCESS\_STEP\_TYPES](#process_step_types)
      - [Possible Values](#possible-values-1)
    - [PROCESS\_STEPS](#process_steps)
    - [PROCESS\_TYPES](#process_types)
      - [Possible Values](#possible-values-2)
    - [PROCESSES](#processes)
    - [TECHNICAL\_USER](#technical_user)
    - [TENANTS](#tenants)
    - [Enum Value Tables](#enum-value-tables)
    - [Process Handling](#process-handling)
  - [NOTICE](#notice)

## Database Overview

```mermaid
erDiagram
    PROCESS_STEP_STATUSES {
        integer id PK
        text label PK
    }
    PROCESS_STEP_TYPES {
        integer id PK
        text label
    }
    PROCESS_STEPS {
        uuid id PK
        integer process_step_type_id FK
        integer process_step_status_id FK
        uuid process_id FK
        timestamp date_created
        timestamp date_last_changed
        text message
    }
    PROCESS_TYPES {
        integer id PK
        text label
    }
    PROCESSES {
        uuid id PK
        integer process_type_id FK
        timestamp lock_expiry_date
        uuid version
    }
    TECHNICAL_USER {
        uuid id PK
        uuid tenant_id FK
        uuid external_id
        text technical_user_name
        text token_address
        text client_id
        bytea client_secret
        bytea initialization_vector
        integer encryption_mode
        uuid process_id FK
    }
    TENANTS {
        uuid id PK
        text company_name
        text bpn
        text did_document_location
        bool is_issuer
        uuid process_id FK
        uuid sub_account_id
        text service_instance_id
        text service_binding_name
        uuid space_id
        uuid dim_instance_id
        text did_download_url
        text did
        text application_id
        uuid company_id
        text application_key
        uuid operator_id
    }
```

## Database Structure

The database is organized into several key tables, each serving a specific purpose:

### PROCESS_STEP_STATUSES

id (INTEGER): A unique identifier for the process step status. This is the primary key of the table.
label (TEXT): The label of the process step status.

#### Possible Values

- `TODO`: The process step is still to be executed.
- `DONE`: The process step was already executed successfully.
- `SKIPPED`: The execution of the process step was skipped.
- `FAILED`: The process step execution failed due to an error.
- `DUPLICATE`: The process step did already exist.

### PROCESS_STEP_TYPES

id (INTEGER): A unique identifier for the process step type. This is the primary key of the table.
label (TEXT): The label of the process step type.

#### Possible Values

- `CREATE_SUBACCOUNT`: Creates the sub account in sap
- `CREATE_SERVICEMANAGER_BINDINGS`: Creates the service manager binding for the created subaccount
- `ASSIGN_ENTITLEMENTS`: Assigns the entitlements
- `CREATE_SERVICE_INSTANCE`: Creates the service instance
- `CREATE_SERVICE_BINDING`: Creates the service binding for the created service instance
- `SUBSCRIBE_APPLICATION`: Subscribes to the `decentralized-identity-management-app` application
- `CREATE_CLOUD_FOUNDRY_ENVIRONMENT`: Creates the cloud foundry environment
- `CREATE_CLOUD_FOUNDRY_SPACE`: Creates the cloud foundry space for the created environment
- `ADD_SPACE_MANAGER_ROLE`: Adds the space manager role for the created subaccount
- `ADD_SPACE_DEVELOPER_ROLE`: Adds the space developer role for the created subaccount
- `CREATE_DIM_SERVICE_INSTANCE`: Creates the dim instance
- `CREATE_SERVICE_INSTANCE_BINDING`: Creates the binding for to the created dim instance
- `GET_DIM_DETAILS`: Retrieves the dim details from SAP Dim
- `CREATE_APPLICATION`: Creates the application in the wallet
- `CREATE_COMPANY_IDENTITY`: Creates a company identity for the wallet
- `ASSIGN_COMPANY_APPLICATION`: Assigns the company identity to the application
- `CREATE_STATUS_LIST`: Creates a statuslist for a company
- `SEND_CALLBACK`: Sends the callback to the portal to transmit the data of the created wallet and did
- `CREATE_TECHNICAL_USER`: Creates a new technical user for a wallet
- `GET_TECHNICAL_USER_DATA`: Retrieves the technical user data from the SAP Dim
- `SEND_TECHNICAL_USER_CREATION_CALLBACK`: Sends the technical user data back to the portal
- `DELETE_TECHNICAL_USER`: Deletes the technical user from the database and from the SAP Dim
- `SEND_TECHNICAL_USER_DELETION_CALLBACK`: Sends a status to the portal if the deletion was successful

### PROCESS_STEPS

id (UUID): A unique identifier for the process step. This is the primary key of the table.
process_step_type_id (INTEGER): A foreign key referencing id in the PROCESS_STEP_TYPES table.
process_step_status_id (INTEGER): A foreign key referencing id in the PROCESS_STEP_STATUSES table.
process_id (UUID): A foreign key referencing id in the PROCESSES table.
date_created (TIMESTAMP): The timestamp when the process step was created.
date_last_changed (TIMESTAMP): The timestamp when the process step was last changed.
message (TEXT): A message associated with the process step.

### PROCESS_TYPES

id (INTEGER): A unique identifier for the process type. This is the primary key of the table.
label (TEXT): The label of the process type.

#### Possible Values

- `SETUP_DIM`: Process to create wallets.
- `TECHNICAL_USER`: Process to create and delete technical users.

### PROCESSES

id (UUID): A unique identifier for the process. This is the primary key of the table.
process_type_id (INTEGER): A foreign key referencing id in the PROCESS_TYPES table.
lock_expiry_date (TIMESTAMP): The lock expiry date of the process.
version (UUID): The version of the process.

### TECHNICAL_USER

id (UUID): A unique identifier for the technical user. This is the primary key of the table
tenant_id (UUID): A unique identifier for the tenant. This is a foreign key referencing id in the TENANT table
external_id (UUID): the id of the technical user in the dim
technical_user_name (TEXT): The name of the technical user
token_address (TEXT): The address for the authentication of the technical user
client_id (TEXT): The client id which is needed for authentication
client_secret (BYTEA): The encrypted client secret
initialization_vector (BYTEA): The used initialization vector which is needed for decrypting the secret
encryption_mode (INTEGER): The used encryption mode for the secret
process_id (UUID): A unique identifier for the process. This is a foreign key referencing id in the PROCESS table

### TENANTS

id (UUID): A unique identifier for the technical user. This is the primary key of the table
company_name (TEXT): Name of the company must be unique in combination with the bpn
bpn (TEXT): Bpn of the company must be unique in combination with the name
did_document_location (TEXT): The location of the did document (url)
is_issuer (BOOL): Defines if the requesting tenant is an issuer
process_id (UUID): A unique identifier for the process. This is a foreign key referencing id in the PROCESS table
sub_account_id (UUID): A unique identifier of the sub account in the SAP DIM
service_instance_id (TEXT): A unique identifier of the service instance id in the SAP DIM
service_binding_name (TEXT): The service binding name in the SAP DIM
space_id (UUID): A unique identifier of the space id in the SAP DIM
dim_instance_id (UUID): A unique identifier of the dim instance in the SAP DIM
did_download_url (TEXT): The url of the did document.
did (TEXT): The did of the wallet
application_id (TEXT): A unique identifier of the application in the SAP DIM
company_id (UUID): A unique identifier of the company in the SAP DIM
application_key (TEXT): The key of the application in the SAP DIM
operator_id (UUID): A unique identifier of the operator which is used for the wallet creation

### Enum Value Tables

`process_step_statuses`, `process_step_types`, `process_steps`, `process_types` are tables designed to store enum values. They contain an id and label, derived from the backend enums.

### Process Handling

The tables `processes`, `process_steps` are used for the processing of the wallet creation and technical user management.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer
