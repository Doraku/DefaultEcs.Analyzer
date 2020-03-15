# Diagnostic Analyzers

ID | Title | Category
---- | --- | --- |
[DEA0001](DEA0001.md) | SubscribeAttribute used on an invalid method | Runtime Error
[DEA0002](DEA0002.md) | WithPredicateAttribute used on an invalid method | Runtime Error
[DEA0003](DEA0003.md) | WithPredicateAttribute used on a method which is not a member of DefaultEcs.System.AEntitySystem or DefaultEcs.System.AEntityBufferedSystem | Correctness
[DEA0004](DEA0004.md) | Component attribute used on a type which is not derived from DefaultEcs.System.AEntitySystem or DefaultEcs.System.AEntityBufferedSystem | Correctness

# Diagnostic Suppressors

ID | Suppressed ID | Justification
---- | --- | --- |
[DES0001](DES0001.md) | IDE0051 | Private member is used by reflection.
[DES0002](DES0002.md) | IDE0051 | Private member is used by reflection.
[DES0003](DES0003.md) | RCS1242 | Signature is dictated by IPublisher.