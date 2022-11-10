# Weak Dictionary

A .NET implementation of a dictionary having weak references to its values. The dictionary automatically removes entries from itself as the values get garbage collected.

See [the article](https://dev.to/bhaeussermann/creating-a-weak-dictionary-in-net-1fo) for more detail.

The library is available as a [NuGet package](https://www.nuget.org/packages/BernhardHaus.Collections.WeakDictionary).

The project targets .NET Standard 2.0 and is confirmed (tested) to be working with .NET 5.0 - 6.0 and .NET Framework 4.6.1 - 4.8
