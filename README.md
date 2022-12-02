# HttpRangeServer

A quick and dirty HTTP server used to move disk image files from a PC to an Apple IIgs using [NetDisk](https://sheumann.github.io/NetDisk/).

## Build
Build HttpRangeServer.sln with .NET 6.0.

`dotnet build src\HttpRangeServer.sln`

## Options
```
  --path <path>  Path to directory of images   (default: current directory)
  --port <port>  TCP port to listen on         (default: 1165)
  --nocache      Do not cache chunks in memory (default: caching enabled)
  --help
```
