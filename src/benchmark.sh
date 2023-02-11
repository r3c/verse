#!/bin/sh

dotnet run --configuration Release --framework net7.0 --project Verse.Benchmark -- --filter '*' --runtimes net7.0
