The answer must come from the installed package, not from memory.

Pass if the response:
- locates the installed BytLabs version (Directory.Packages.props or a .csproj)
- reads lib/net8.0/BytLabs.Application.xml (or docs/BytLabs.Application.md) from the
  NuGet cache rather than asserting the signature outright
- reports the assembly, Assembly[] and optional MediatR configuration parameters
- states which version the answer applies to

Fail if it states a signature with no evidence of reading the package, if it proposes
decompiling the DLL, or if it invents parameters.
