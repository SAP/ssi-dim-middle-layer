# Development Concept

## Build, test, deploy

Details to the build, test and deploy process can get found under the following md file: [Release Process](/docs/technical-documentation/release-process/Release%20Process.md)

## Development Guidelines

The SSI DIM middle layer is using following key frameworks:

- .Net
- Entity Framework

### Swagger

The API uses OpenAPI annotations to describe the endpoints with all necessary information. The annotations are then used to automatically generate the OpenAPI specification file, which can be viewed in the Swagger UI that is deployed with the application.

#### API Dev Guidelines

##### Implement authorization

API's need to ensure that they only grant access to the authorized requester. For example, a user might be approved to access the API, but if they’re not allowed to add information to the application’s database via the POST method, any request to do so should be rejected. Authorization information can also be contained within a request as a token.

Unlike some other API types, REST APIs must authenticate and authorize each request made to the server, even if multiple requests come from the same user. This is because REST communications are stateless — that is, each request can be understood by the API in isolation, without information from previous requests.

Authorization can be governed by user roles, where each role comes with different permissions. Generally, API developers should adhere to the principle of least privilege, which states that users should only have access to the resources and methods necessary for their role, and nothing more. Predefined roles make it easier to oversee and change user permissions, reducing the chance that a bad actor can access sensitive data.

In terms of implementation all endpoints should be secured with the highest restrictions as default. Restrictions should only be lessened through explicit exemptions. This ensures that in case of oversights an endpoint can be more secured than intended but never less secured.

##### Validate all requests

As mentioned, sometimes requests from perfectly valid sources may be hacking attempts. Therefore, APIs need rules to determine whether a request is friendly, friendly but invalid, or harmful, like an attempt to inject harmful code.

An API request is only processed once its contents pass a thorough validation check — otherwise, the request should never reach the application data layer.

Validation also includes sanity checks: Define sensible value ranges for the parameters a user provides. This especially is valid for the size of the request and the response. APIs should limit the possible number of records to process in order to prevent intentional or unintentional overloads of the system.

##### Encrypt all requests and responses

To prevent MITM attacks, any data transfer from the user to the API server or vice versa must be properly encrypted. This way, any intercepted requests or responses are useless to the intruder without the right decryption method.

Since REST APIs use HTTP, encryption can be achieved by using the Transport Layer Security (TLS) protocol or Secure Sockets Layer (SSL) protocol. These protocols supply the S in “HTTPS” (“S” meaning “secure'') and are the standard for encrypting web pages and REST API communications.

TLS/SSL only encrypts data when that data is being transferred. It doesn’t encrypt data sitting behind your API, which is why sensitive data should also be encrypted in the database layer as well.

##### Only include necessary information in responses

Like you might unintentionally let a secret slip when telling a story to a friend, it’s possible for an API response to expose information hackers can use. To prevent this, all responses sent to the end-user should include only the information to communicate the success or failure of the request, the resource requested (if any), and any other information directly related to these resources.

In other words, avoid “oversharing” data — the response is a chance for you to inadvertently expose private data, either through the returned resources or verbose status messages.

=> in the ownership of every API Developer

##### Throttle API requests and establish quotas

To prevent brute-force attacks like DDoS, an API can impose rate-limiting, a way to control the number of requests to the API server at any given time.

There are two main ways to rate-limit API requests, quotas and throttling. Quotas limit the number of requests allowed from a user over a span of time, while throttling slows a user’s connection while still allowing them to use your API.

Both methods should allow normal API requests but prevent floods of traffic intended to disrupt, as well as unexpected request spikes in general.

##### Log API activity

Logging API activities is extremely important when it comes to tracing user activity and in worst case hack activity.

###### Conduct security tests

=> see [Test Section](#tests) below

##### Error Handling

The simplest way we handle errors is to respond with an appropriate status code.

Common agreed response codes:

- 400 Bad Request – client sent an invalid request, such as lacking required request body or parameter.
  Example: The same constraint has been configured multiple times in the request
- 401 Unauthorized – user authenticated but doesn't have permission to access the requested resource.
  Example: User token doesn't have the access on the resource.
- 403 Forbidden – client failed to authenticate with the server.
  Example: token expired oder invalid login.
- 404 Not Found – the requested resource does not exist.
  Example: A tenant can not be found.
- 500 Internal Server Error – a generic error occurred in the internal system logic.
  Example: Unexpected server-side issue during wallet creation.
  Additionally to the generic error code, a detailed message/error is needed to ensure that the issue can get validated and resolved quickly.

##### Repository Pattern

The repositories are used via the Factory DimRepositories, which ensures that the same database instance is used for all repositories.

Furthermore, it provides an implicit transaction functionality.

The repositories themselves must not be registered for dependency injection in the corresponding startup; the method DimRepositories.GetInstance<RepositoryType> provides the instance of a requested repository.

In the repository itself, you should not work with SaveChanges, it should only be called via the IssuerRepositories.SaveChanges to ensure that any transaction dependencies can be rolled back.

#### Tests

##### User Authentication Test

If authentication mechanisms are implemented incorrectly, attackers can compromise authentication tokens or exploit implementation flaws to assume other users’ identities and gain access to your API’s endpoints.

To test your authentication mechanisms, try sending API requests without proper authentication (either no tokens or credentials, or incorrect ones) and see if your API responds with the correct error and messaging.

##### Parameter Tampering Test

To run a parameter tampering test, try various combinations of invalid query parameters in your API requests and see if it responds with the correct error codes. If not, then your API likely has some backend validation errors that need to be resolved.

##### Injection Test

To test if your API is vulnerable to injections, try injecting SQL, NoSQL, LDAP, OS, or other commands in API inputs and see if your API executes them. These commands should be harmless, like reboot commands or cat commands.

##### Unhandled HTTP Methods Test

Most APIs have various HTTP methods that are used to retrieve, store, or delete data. Sometimes web servers will give access to unsupported HTTP methods by default, which makes your API vulnerable.

To test for this vulnerability, you should try all the common HTTP methods (POST, GET, PUT, PATCH, and DELETE) as well as a few uncommon ones. TRY sending an API request with the HEAD verb instead of GET, for example, or a request with an arbitrary method like FOO. You should get an error code, but if you get a 200 OK response, then your API has a vulnerability.

##### Load Test

Load testing should be one of the last steps of your API security auditing process. This type is pushing the API to its limits in order to discover any functional or security issues that have yet to be revealed.

To achieve this, send a large number of randomized requests, including SQL queries, system commands, arbitrary numbers, and other non-text characters, and see if your API responds with errors, processes any of these inputs incorrectly, or crashes. This type of testing will mimic Overflow and DDoS attacks.

An API manager or gateway tool will handle or help address the API security guidelines described above (including testing).

## Migration

To run the SSI DIM middle layer, migrations are needed to load the initial data inside the SSI DIM middle layer db to enable the SSI DIM middle layer to work.
The migration will consist of an initial migration as well as delta migration files with future releases. As part of a new release, a migration file (if applicable) will get released and can get loaded via a delta load.

## Configurability

SSI DIM Middle Layer configuration is mainly possible via the appsettings files as well as the static data migration files.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 SAP SE or an SAP affiliate company, BMW Group AG and ssi-dim-middle-layer contributors
- Source URL: https://github.com/SAP/ssi-dim-middle-layer
