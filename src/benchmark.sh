#!/bin/sh

dotnet run --configuration Release --framework net9.0 --project Verse.Benchmark -- --filter '*' --runtimes net90
