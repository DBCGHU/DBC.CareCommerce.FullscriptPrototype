# Fullscript Integration Next Steps

## Current dispatch contract

The current integration dispatch path is intentionally narrow:

- `IFullscriptApiClient.DispatchTreatmentPlan(FullscriptTransactionDto transaction)` is the single outbound API contract.
- `FullscriptTransactionDispatcherService` loads `ReadyToSend` transactions and marks each transaction `Sent` or `Failed`.
- `FullscriptHttpApiClient` currently builds a treatment plan request from fields already present on `FullscriptTransactionDto`.

## Fields required before dispatch

A transaction cannot be dispatched unless these values are present:

- `PatientId`
- `CareItemId`
- `CatalogItemId`
- `FullscriptPatientId`
- `FullscriptPractitionerId`
- `FullscriptProductId`
- `FullscriptVariantId`

## Important data gap

`FullscriptPatientId` and `FullscriptPractitionerId` are persisted on `dbo.FullscriptTransaction`, but this prototype does not yet include a workflow that looks up, creates, or maps those IDs from local patient/provider data.

The next real API integration milestone should be one of the following:

1. Add a mapping workflow that stores known Fullscript patient/practitioner IDs before a transaction reaches `ReadyToSend`.
2. Add Fullscript patient lookup/create support before treatment plan dispatch.
3. Add Fullscript practitioner lookup/configuration support, likely from tenant or provider-level settings, before treatment plan dispatch.

## Recommended next implementation order

1. Decide whether `FullscriptPractitionerId` is a static practice/provider configuration value or a per-provider mapped value.
2. Decide whether `FullscriptPatientId` will be pre-mapped or created/located through the Fullscript API during dispatch.
3. Add explicit request/response DTOs for the treatment plan payload instead of using anonymous objects.
4. Replace placeholder endpoint paths only after confirming the exact Fullscript API endpoint names and payload contract.

## Safety rule

Do not mark a transaction `Sent` unless Fullscript returns a durable treatment plan identifier that can be stored in `FullscriptTreatmentPlanId`.
