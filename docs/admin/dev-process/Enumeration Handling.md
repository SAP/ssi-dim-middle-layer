# Enumeration

Enum or enumeration are used for data type consisting of named values like elements, status workflow, types, etc., that represent integral constants. Enums are non-transactional (so called static data) which can only get changed in a new application version. Changes in the operation mode of an application are not allowed since this will result into possible system breaks.

List of used enums in the ssi dim middle layer application

- process_step_statuses
- process_step_types
- process_steps
- process_types

## Add Enums

New enums can get added easily be enhancing the enumeration table (via the seeding data). With the next deployment; the new enum is getting auto deployed to the respective env.
Since enums have an enhanced impact on the system functionality; it is mandatorily needed to test (FE wise) the impacted screens / flows before releasing new enums. It is likely that the enum has an enhanced impact on the user journey / flow and break the system if not well tested.

## Change Enums

Change of enums (labels) is possible but need to be done carefully and only if necessarily needed.
In the case a change is getting executed; the system configuration / appsettings / env. variables need to get checked to ensure that those don't refer to the enum which is getting changed/ updated.

## Delete Enums

Deletion of enums have following impacts

- Seeding data update needed (likely data need to get deleted / changed)
- Data inside the database in the different running environments need to get updated
- User flow process impacted
- Business logic impacted

**It is not recommended to delete enums.**

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer
