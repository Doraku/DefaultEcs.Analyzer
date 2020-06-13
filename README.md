![DefaultEcs](https://github.com/Doraku/DefaultEcs/blob/master/image/DefaultEcsLogo.png)
This project provides a set of analizers to enhance the Visual Studio experience when working on a DefaultEcs project by adding diagnostics or by removing general C# diagnostics that do not apply.  
The main repo for DefaultEcs is [here](https://github.com/Doraku/DefaultEcs/), this repo is specific to the analyzer.

[Here](https://github.com/Doraku/DefaultEcs.Analyzer/blob/master/documentation/index.md) is the list of analyzers and suppressors defined in this project.

[![NuGet](https://buildstats.info/nuget/DefaultEcs)](https://www.nuget.org/packages/DefaultEcs.Analyzer)
[![Join the chat at https://gitter.im/Doraku/DefaultEcs](https://badges.gitter.im/Doraku/DefaultEcs.svg)](https://gitter.im/Doraku/DefaultEcs?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

- [Release note](./documentation/RELEASENOTE.md 'Release note')
<a/>

- [Requirement](#Requirement)
- [Versioning](#Versioning)

<a name='Requirement'></a>
# Requirement

<a name='Versioning'></a>
# Versioning
This is the current strategy used to version DefaultEcs.Analyzer: v0.major.minor
- 0: DefaultEcs is still in heavy development and although a lot of care is given to not break the current api, it can still happen
- major: incremented when there is a breaking change (reset minor number)
- minor: incremented when there is a new feature or a bug fix