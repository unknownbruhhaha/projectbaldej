#!/bin/bash
rm -R obj/
rm -R BaldejFramework/obj/
dotnet build
cp assets -R bin/Debug/net7.0/assets
