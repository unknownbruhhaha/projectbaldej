#!/bin/bash
rm -R obj/
rm -R BaldejFramework/obj/
dotnet build -c Release
cd bin/Release/net7.0/
rm assets
rm libNewton.so
ln -s ../../../assets assets
ln -s ../../../libNewton.so libNewton.so

