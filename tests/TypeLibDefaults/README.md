# Type library default-value regression tests

On Windows with a .NET SDK supporting C# 10 and .NET Framework 4.8.1 reference assemblies:

```cmd
dotnet run --project tests/TypeLibDefaults -c Release -p:TestArchitecture=x86
dotnet run --project tests/TypeLibDefaults -c Release -p:TestArchitecture=x64
```

The standalone runner checks the native `PARAMDESCEX` size and the offset of its embedded `VARIANT`, then reads integer (including zero), Boolean, floating point, decimal and string defaults. The native buffers use offset 8 independently of the managed structure, so the former pointer-field declaration fails these tests in x86 rather than hiding incorrect null defaults. `VariantClear` releases the test values.

It also parses and formats the Windows stdole, Scripting and Shell type libraries. Additional `.tlb` or DLL paths can be passed after `--` to `dotnet run`. To test an existing binary without rebuilding it, run the generated test executable with `--assembly path\to\OleViewDotNet.exe` followed by optional type-library paths. No custom COM registration or third-party TLB fixture is needed for the default suite.
