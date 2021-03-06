# Diagnostic Analyzers

ID | Title | Category
---- | --- | --- |
[DEA0001](DEA0001.md) | SubscribeAttribute used on an invalid method | Runtime Error
[DEA0002](DEA0002.md) | WithPredicateAttribute used on an invalid method | Runtime Error
[DEA0003](DEA0003.md) | WithPredicateAttribute used on a method which is not a member of AEntitySetSystem or AEntityMultiMapSystem | Correctness
[DEA0004](DEA0004.md) | Component attribute used on a type which is not derived from AEntitySetSystem or AEntityMultiMapSystem | Correctness
[DEA0005](DEA0005.md) | Entity modification methods are not thread safe and should not be used inside the Update method of AEntitySetSystem or AEntityMultiMapSystem | Runtime Error
[DEA0006](DEA0006.md) | The Update attribute should be used on a method of a type which inherit from AEntitySetSystem or AEntityMultiMapSystem | Correctness
[DEA0007](DEA0007.md) | Only one method can be decorated with the Update attribute in a given type | Correctness
[DEA0008](DEA0008.md) | The Update attribute can't be used when an override of the Update method is already present | Correctness
[DEA0009](DEA0009.md) | No out parameter can be present in the method decorated by the Update attribute | Correctness
[DEA0010](DEA0010.md) | The type containing the method decorated by the Update attribute should be partial | Correctness
[DEA0011](DEA0011.md) | The method decorated by the Update attribute should return void | Correctness
[DEA0012](DEA0012.md) | The method decorated by the Update attribute should not be generic | Correctness
[DEA0013](DEA0013.md) | The ConstructorParameter attribute should be used on a member of a type which inherit from AEntitySetSystem or AEntityMultiMapSystem | Correctness
[DEA0014](DEA0014.md) | The ConstructorParameter attribute should be used on a member of a type which has a method with a Update attribute | Correctness
[DEA0015](DEA0015.md) | The ConstructorParameter attribute should be used on a member of a type which has no constructor defined | Correctness
[DEA0016](DEA0016.md) | The method of a parameter decorated with a Added or Changed attribute should be decorated wit the Update attribute | Correctness

# Diagnostic Suppressors

ID | Suppressed ID | Justification
---- | --- | --- |
[DES0001](DES0001.md) | IDE0051 | Private member is used by reflection.
[DES0002](DES0002.md) | IDE0051 | Private member is used by reflection.
[DES0003](DES0003.md) | RCS1242 | Signature is dictated by IPublisher.
[DES0004](DES0004.md) | RCS1242 | Signature is dictated by its usage as a ComponentPredicate.
[DES0005](DES0005.md) | RCS1043 | Partial class generated.
[DES0006](DES0006.md) | IDE0051 | Partial class generated.
[DES0006](DES0007.md) | RCS1242 | More explicit.