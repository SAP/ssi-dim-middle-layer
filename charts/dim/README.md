# Helm chart for DIM Middle Layer

This helm chart installs the DIM Middle Layer.

For further information please refer to [Technical Documentation](./docs/technical-documentation).

The referenced container images are for demonstration purposes only.

## Installation

To install the chart with the release name `dim`:

```shell
$ helm repo add ssi-dim-middle-layer https://sap.github.io/ssi-dim-middle-layer
$ helm install dim ssi-dim-middle-layer/dim
```

To install the helm chart into your cluster with your values:

```shell
$ helm install -f your-values.yaml dim ssi-dim-middle-layer/dim
```

To use the helm chart as a dependency:

```yaml
dependencies:
  - name: dim
    repository: https://sap.github.io/ssi-dim-middle-layer
    version: 1.1.0
```

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 12.12.x |

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| dim.image.name | string | `"ghcr.io/sap/dim-client_dim-service"` |  |
| dim.image.tag | string | `""` |  |
| dim.imagePullPolicy | string | `"IfNotPresent"` |  |
| dim.resources | object | `{"limits":{"cpu":"45m","memory":"300M"},"requests":{"cpu":"15m","memory":"300M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| dim.healthChecks.startup.path | string | `"/health/startup"` |  |
| dim.healthChecks.startup.tags[0].name | string | `"HEALTHCHECKS__0__TAGS__1"` |  |
| dim.healthChecks.startup.tags[0].value | string | `"dimdb"` |  |
| dim.healthChecks.liveness.path | string | `"/healthz"` |  |
| dim.healthChecks.readyness.path | string | `"/ready"` |  |
| dim.swaggerEnabled | bool | `false` |  |
| dim.rootDirectoryId | string | `"00000000-0000-0000-0000-000000000000"` |  |
| dim.operatorId | string | `"00000000-0000-0000-0000-000000000000"` |  |
| migrations.name | string | `"migrations"` |  |
| migrations.image.name | string | `"ghcr.io/sap/dim-client_dim-migrations"` |  |
| migrations.image.tag | string | `""` |  |
| migrations.imagePullPolicy | string | `"IfNotPresent"` |  |
| migrations.resources | object | `{"limits":{"cpu":"45m","memory":"200M"},"requests":{"cpu":"15m","memory":"200M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| migrations.seeding.testDataEnvironments | string | `""` |  |
| migrations.seeding.testDataPaths | string | `"Seeder/Data"` |  |
| migrations.logging.default | string | `"Information"` |  |
| processesworker.name | string | `"processesworker"` |  |
| processesworker.image.name | string | `"ghcr.io/sap/dim-client_dim-processes-worker"` |  |
| processesworker.image.tag | string | `""` |  |
| processesworker.imagePullPolicy | string | `"IfNotPresent"` |  |
| processesworker.resources | object | `{"limits":{"cpu":"45m","memory":"300M"},"requests":{"cpu":"15m","memory":"300M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| processesworker.dim.adminMail | string | `"mail@example.org"` |  |
| processesworker.dim.clientIdCisCentral | string | `""` |  |
| processesworker.dim.clientSecretCisCentral | string | `""` |  |
| processesworker.dim.authUrl | string | `""` |  |
| processesworker.subaccount.baseUrl | string | `""` | Url to the subaccount service api |
| processesworker.entitlement.baseUrl | string | `""` | Url to the entitlement service api |
| processesworker.cf.clientId | string | `""` |  |
| processesworker.cf.clientSecret | string | `""` |  |
| processesworker.cf.tokenAddress | string | `""` |  |
| processesworker.cf.baseUrl | string | `""` | Url to the cf service api |
| processesworker.cf.grantType | string | `"client_credentials"` |  |
| processesworker.callback.scope | string | `"openid"` |  |
| processesworker.callback.grantType | string | `"client_credentials"` |  |
| processesworker.callback.clientId | string | `""` | Provide client-id for callback. |
| processesworker.callback.clientSecret | string | `""` | Client-secret for callback client-id. Secret-key 'callback-client-secret'. |
| processesworker.callback.tokenAddress | string | `""` |  |
| processesworker.callback.baseAddress | string | `""` | Url to the cf service api |
| processesworker.technicalUserCreation.encryptionConfigIndex | int | `0` |  |
| processesworker.technicalUserCreation.encryptionConfigs.index0.index | int | `0` |  |
| processesworker.technicalUserCreation.encryptionConfigs.index0.cipherMode | string | `"CBC"` |  |
| processesworker.technicalUserCreation.encryptionConfigs.index0.paddingMode | string | `"PKCS7"` |  |
| processesworker.technicalUserCreation.encryptionConfigs.index0.encryptionKey | string | `""` | EncryptionKey to encrypt the technical user client-secret. Secret-key 'technicalusercreation-encryption-key0'. Expected format is 256 bit (64 digits) hex. |
| existingSecret | string | `""` | Secret containing "client-secret-cis-central", "client-secret-cf" and "client-secret-callback" |
| dotnetEnvironment | string | `"Production"` |  |
| dbConnection.schema | string | `"dim"` |  |
| dbConnection.sslMode | string | `"Disable"` |  |
| postgresql.enabled | bool | `true` | PostgreSQL chart configuration; default configurations: host: "dim-postgresql-primary", port: 5432; Switch to enable or disable the PostgreSQL helm chart. |
| postgresql.image | object | `{"tag":"15-debian-12"}` | Setting image tag to major to get latest minor updates |
| postgresql.commonLabels."app.kubernetes.io/version" | string | `"15"` |  |
| postgresql.auth.username | string | `"dim"` | Non-root username. |
| postgresql.auth.database | string | `"dim"` | Database name. |
| postgresql.auth.existingSecret | string | `"{{ .Release.Name }}-dim-postgres"` | Secret containing the passwords for root usernames postgres and non-root username dim. Should not be changed without changing the "dim-postgresSecretName" template as well. |
| postgresql.auth.postgrespassword | string | `""` | Password for the root username 'postgres'. Secret-key 'postgres-password'. |
| postgresql.auth.password | string | `""` | Password for the non-root username 'dim'. Secret-key 'password'. |
| postgresql.auth.replicationPassword | string | `""` | Password for the non-root username 'repl_user'. Secret-key 'replication-password'. |
| postgresql.architecture | string | `"replication"` |  |
| postgresql.audit.pgAuditLog | string | `"write, ddl"` |  |
| postgresql.audit.logLinePrefix | string | `"%m %u %d "` |  |
| postgresql.primary.extendedConfiguration | string | `""` | Extended PostgreSQL Primary configuration (increase of max_connections recommended - default is 100) |
| postgresql.primary.initdb.scriptsConfigMap | string | `"{{ .Release.Name }}-dim-cm-postgres"` |  |
| postgresql.readReplicas.extendedConfiguration | string | `""` | Extended PostgreSQL read only replicas configuration (increase of max_connections recommended - default is 100) |
| externalDatabase.host | string | `"dim-postgres-ext"` | External PostgreSQL configuration IMPORTANT: non-root db user needs to be created beforehand on external database. And the init script (02-init-db.sql) available in templates/configmap-postgres-init.yaml needs to be executed beforehand. Database host ('-primary' is added as postfix). |
| externalDatabase.port | int | `5432` | Database port number. |
| externalDatabase.username | string | `"dim"` | Non-root username for dim. |
| externalDatabase.database | string | `"dim"` | Database name. |
| externalDatabase.password | string | `""` | Password for the non-root username (default 'dim'). Secret-key 'password'. |
| externalDatabase.existingSecret | string | `"dim-external-db"` | Secret containing the password non-root username, (default 'dim'). |
| idp | object | `{"address":"https://centralidp.example.org","authRealm":"CX-Central","jwtBearerOptions":{"metadataPath":"/auth/realms/CX-Central/.well-known/openid-configuration","refreshInterval":"00:00:30","requireHttpsMetadata":"true","tokenValidationParameters":{"validAudience":"DIM-Middle-Layer","validIssuerPath":"/auth/realms/CX-Central"}},"tokenPath":"/auth/realms/CX-Central/protocol/openid-connect/token","useAuthTrail":true}` | Provide details about idp instance. |
| idp.address | string | `"https://centralidp.example.org"` | Provide idp base address, without trailing '/auth'. |
| idp.useAuthTrail | bool | `true` | Flag if the api should be used with an leading /auth path |
| ingress.enabled | bool | `false` | DIM ingress parameters, enable ingress record generation for dim. |
| ingress.tls[0] | object | `{"hosts":[""],"secretName":""}` | Provide tls secret. |
| ingress.tls[0].hosts | list | `[""]` | Provide host for tls secret. |
| ingress.hosts[0] | object | `{"host":"","paths":[{"backend":{"port":8080},"path":"/api/dim","pathType":"Prefix"}]}` | Provide default path for the ingress record. |
| portContainer | int | `8080` |  |
| portService | int | `8080` |  |
| replicaCount | int | `3` |  |
| nodeSelector | object | `{}` | Node labels for pod assignment |
| tolerations | list | `[]` | Tolerations for pod assignment |
| affinity.podAntiAffinity | object | `{"preferredDuringSchedulingIgnoredDuringExecution":[{"podAffinityTerm":{"labelSelector":{"matchExpressions":[{"key":"app.kubernetes.io/name","operator":"DoesNotExist"}]},"topologyKey":"kubernetes.io/hostname"},"weight":100}]}` | Following Catena-X Helm Best Practices, [reference](https://kubernetes.io/docs/concepts/scheduling-eviction/assign-pod-node/#affinity-and-anti-affinity). |
| updateStrategy.type | string | `"RollingUpdate"` | Update strategy type, rolling update configuration parameters, [reference](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#update-strategies). |
| updateStrategy.rollingUpdate.maxSurge | int | `1` |  |
| updateStrategy.rollingUpdate.maxUnavailable | int | `0` |  |
| startupProbe | object | `{"failureThreshold":30,"initialDelaySeconds":10,"periodSeconds":10,"successThreshold":1,"timeoutSeconds":1}` | Following Catena-X Helm Best Practices, [reference](https://github.com/helm/charts/blob/master/stable/nginx-ingress/values.yaml#L210). |
| livenessProbe.failureThreshold | int | `3` |  |
| livenessProbe.initialDelaySeconds | int | `10` |  |
| livenessProbe.periodSeconds | int | `10` |  |
| livenessProbe.successThreshold | int | `1` |  |
| livenessProbe.timeoutSeconds | int | `10` |  |
| readinessProbe.failureThreshold | int | `3` |  |
| readinessProbe.initialDelaySeconds | int | `10` |  |
| readinessProbe.periodSeconds | int | `10` |  |
| readinessProbe.successThreshold | int | `1` |  |
| readinessProbe.timeoutSeconds | int | `1` |  |

Autogenerated with [helm docs](https://github.com/norwoodj/helm-docs)
