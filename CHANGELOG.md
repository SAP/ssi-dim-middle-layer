# Changelog

## [2.2.2](https://github.com/SAP/ssi-dim-middle-layer/compare/v2.1.1...v2.2.2) (2025-05-08)

### Bug Fixes

* **errorHandling:** adjust general error handler ([#165](https://github.com/SAP/ssi-dim-middle-layer/issues/165)) ([1c32a68](https://github.com/SAP/ssi-dim-middle-layer/commit/1c32a68aa0654fe4c0b3f95a9cedfc413477102d))

## [2.2.1](https://github.com/SAP/ssi-dim-middle-layer/compare/v2.2.0...v2.2.1) (2025-02-07)

### Bug Fixes

* **processWorker:** add registration for process identity ([#158](https://github.com/SAP/ssi-dim-middle-layer/issues/158)) ([c644b2b](https://github.com/SAP/ssi-dim-middle-layer/commit/c644b2bc06477a5cc5a657de93127493e0171b56))

## [2.2.0](https://github.com/SAP/ssi-dim-middle-layer/compare/v2.1.0...v2.2.0) (2025-02-07)

### Features

* **processes:** use process package ([#145](https://github.com/SAP/ssi-dim-middle-layer/issues/145)) ([17eb127](https://github.com/SAP/ssi-dim-middle-layer/commit/17eb1271cb018eb448ef91e119ad86353440fab1)), closes [#3](https://github.com/SAP/ssi-dim-middle-layer/issues/3)
* add support for bitstring statuslist ([#125](https://github.com/SAP/ssi-dim-middle-layer/issues/125)) ([3c96cbc](https://github.com/SAP/ssi-dim-middle-layer/commit/3c96cbcfceb74e76c98f421574d028ca2486a7d1)), closes [#80](https://github.com/SAP/ssi-dim-middle-layer/issues/80)
* upgrade .NET to v9 ([#150](https://github.com/SAP/ssi-dim-middle-layer/issues/150)) ([8720943](https://github.com/SAP/ssi-dim-middle-layer/commit/87209434beb94cc11ab2abf4f8a48e78caae3a19))

## [2.1.1](https://github.com/SAP/ssi-dim-middle-layer/compare/v2.1.0...v2.1.1) (2024-10-24)

### Bug Fixes

* **processes** fix retrigger endpoint ([#124](https://github.com/SAP/ssi-dim-middle-layer/issues/124)) ([f33f664](https://github.com/SAP/ssi-dim-middle-layer/commit/f33f6645a347ed7d45e839fb848081d7f2f522af)), closes [#111](https://github.com/SAP/ssi-dim-middle-layer/issues/111)
* **environment** adjust int configuration ([#123](https://github.com/SAP/ssi-dim-middle-layer/issues/123)) ([4be9406](https://github.com/SAP/ssi-dim-middle-layer/commit/4be9406e865a6147c01535c46fb77380d8e5db8d))

## [2.1.0](https://github.com/SAP/ssi-dim-middle-layer/compare/v2.0.0...v2.1.0) (2024-10-24)

### Features

* **seeding:** add initial wallet seeding ([#109](https://github.com/SAP/ssi-dim-middle-layer/issues/109)) ([8cfb9a0](https://github.com/SAP/ssi-dim-middle-layer/commit/8cfb9a0ca530c5a77c38b49246b67c5a19fbe1b1)), closes [#108](https://github.com/SAP/ssi-dim-middle-layer/issues/108)

### Bug Fixes

* adjust name check for tenant and technical user ([#113](https://github.com/SAP/ssi-dim-middle-layer/issues/113)) ([cf91f31](https://github.com/SAP/ssi-dim-middle-layer/commit/cf91f31ae9477718a556ef64b533511bc3d43f63)), closes [#112](https://github.com/SAP/ssi-dim-middle-layer/issues/112)

## [2.0.0](https://github.com/SAP/ssi-dim-middle-layer/compare/v1.2.1...v2.0.0) (2024-10-08)


### ⚠ BREAKING CHANGES

* **provisioning:** change to the new div provisioning api ([#93](https://github.com/SAP/ssi-dim-middle-layer/issues/93))

### Features

* **provisioning:** change to the new div provisioning api ([#93](https://github.com/SAP/ssi-dim-middle-layer/issues/93)) ([bf650d4](https://github.com/SAP/ssi-dim-middle-layer/commit/bf650d40a9e3b1696de54c56a900bbf1dc3a703a)), closes [#79](https://github.com/SAP/ssi-dim-middle-layer/issues/79)
* **wallet:** add existence check for wallet creation ([#71](https://github.com/SAP/ssi-dim-middle-layer/issues/71)) ([c23dfe1](https://github.com/SAP/ssi-dim-middle-layer/commit/c23dfe1039e3b29cee19771be15ea2f3bc9cd7ac)), closes [#66](https://github.com/SAP/ssi-dim-middle-layer/issues/66)


### Bug Fixes

* **callback:** add error handling for callback service ([#73](https://github.com/SAP/ssi-dim-middle-layer/issues/73)) ([9db2959](https://github.com/SAP/ssi-dim-middle-layer/commit/9db295930374a296e2bcd0e1aa8ce9249ac3baf8)), closes [#67](https://github.com/SAP/ssi-dim-middle-layer/issues/67)
* **environment:** adjust getEnvironment exception handling ([#89](https://github.com/SAP/ssi-dim-middle-layer/issues/89)) ([8b526a5](https://github.com/SAP/ssi-dim-middle-layer/commit/8b526a517404dad4b17bdefc05e904d6a526b228)), closes [#84](https://github.com/SAP/ssi-dim-middle-layer/issues/84)
* **tenantName:** remove invalid characters from tenant name ([#88](https://github.com/SAP/ssi-dim-middle-layer/issues/88)) ([d44ef09](https://github.com/SAP/ssi-dim-middle-layer/commit/d44ef0909f5972fb12e99795ef2e0e49402b6cd0))

## [1.2.1](https://github.com/SAP/ssi-dim-middle-layer/compare/v1.2.0...v1.2.1) (2024-08-02)


### Bug Fixes

* adjust variable naming ([#70](https://github.com/SAP/ssi-dim-middle-layer/issues/70)) ([2853060](https://github.com/SAP/ssi-dim-middle-layer/commit/2853060e08ce93cabd5cfde34dda024e47c8c8a1)), closes [#59](https://github.com/SAP/ssi-dim-middle-layer/issues/59)
* **dependencies:** fix high severity finding ([#65](https://github.com/SAP/ssi-dim-middle-layer/issues/65)) ([9616c52](https://github.com/SAP/ssi-dim-middle-layer/commit/9616c52fdfcb4a7d65135e3e36df029c37e8073e)), closes [#71](https://github.com/SAP/ssi-dim-middle-layer/issues/71)

## [1.2.0](https://github.com/SAP/ssi-dim-middle-layer/compare/v1.1.0...v1.2.0) (2024-07-17)


### Features

* add technical user deletion logic and adjust exception handling for encryption ([#50](https://github.com/SAP/ssi-dim-middle-layer/issues/50)) ([a140a48](https://github.com/SAP/ssi-dim-middle-layer/commit/a140a481136eb2e97338b96be5a5732086945a34))

## 1.1.0 (2024-07-04)


### Features

* **ci:** adjust company identity creation ([#36](https://github.com/SAP/ssi-dim-middle-layer/issues/36)) ([e427ebf](https://github.com/SAP/ssi-dim-middle-layer/commit/e427ebfa98391b1bb3304661d7fcfb701e1c7529))
* **technicalUser:** add endpoint to create technicalUser for wallet ([#30](https://github.com/SAP/ssi-dim-middle-layer/issues/30)) ([d8a4d61](https://github.com/SAP/ssi-dim-middle-layer/commit/d8a4d61f107452c3b86d5e9857f3643bb4a3ca27))


### Bug Fixes

* add url encoding to get endpoints ([#44](https://github.com/SAP/ssi-dim-middle-layer/issues/44)) ([6470a57](https://github.com/SAP/ssi-dim-middle-layer/commit/6470a5769116b7bc52a41587bb09df332aae18eb))
* **env:** adjust callback environment ([d75473b](https://github.com/SAP/ssi-dim-middle-layer/commit/d75473b037d306dacc8831f860dd2a0cee46f53c))
* **technicalUser:** fix callback url for technical user ([accfee8](https://github.com/SAP/ssi-dim-middle-layer/commit/accfee8007f368152a440f3d6f7754a5ac15c83e))


### Miscellaneous Chores

* release v1.1.0 ([d2face6](https://github.com/SAP/ssi-dim-middle-layer/commit/d2face64a55f3f94475b892a46e6a46beaa0e465))

## 1.0.0 (2024-04-09)


### Features

* **authorization:** add role authorization ([#19](https://github.com/SAP/ssi-dim-middle-layer/issues/19)) ([221a435](https://github.com/SAP/ssi-dim-middle-layer/commit/221a435c629149e5fadb0514be6a595fe968594a))
* **client:** add dim client ([266e807](https://github.com/SAP/ssi-dim-middle-layer/commit/266e80764e0009be8cdad53781194f837140e151))
* **net8:** upgrade to .net8 ([#23](https://github.com/SAP/ssi-dim-middle-layer/issues/23)) ([d3494de](https://github.com/SAP/ssi-dim-middle-layer/commit/d3494dedf046b05ffe7b346298abbfb2286f452f))
* **statusList:** add statuslist endpoints ([#22](https://github.com/SAP/ssi-dim-middle-layer/issues/22)) ([167ff48](https://github.com/SAP/ssi-dim-middle-layer/commit/167ff48a404b17b226addac5695df02463cd5002))


### Bug Fixes

* **bindings:** adjust errorhandling for service instances ([#29](https://github.com/SAP/ssi-dim-middle-layer/issues/29)) ([5b8f6cc](https://github.com/SAP/ssi-dim-middle-layer/commit/5b8f6cc65a60e42d6791e8e3d5a85bbd2e2dffb3))
