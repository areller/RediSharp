#!/bin/bash

dotnet pack -c Release -p:PackageVersion=$TRAVIS_TAG
dotnet nuget push src/RediSharp/bin/Release/RediSharp.*.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json -s https://github.com/areller/RediSharp