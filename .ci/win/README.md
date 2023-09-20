Sometimes chocolatey wants me to tweak something (file extensions, empty fields in nuspec, etc) and I need to resubmit a specific version. To do so locally use the following from the root dug directory:

1. Update `<version>` tag in dug.nuspec
2. `dotnet publish -r win-x64 -c Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true -o ./.ci/win/output ./cli`
3. `docker run -ti -v $PWD:$PWD -w $PWD linuturk/mono-choco pack ./.ci/win/dug.nuspec --out ./.ci/win`
4. `docker run -ti -v $PWD:$PWD -w $PWD linuturk/mono-choco push ./.ci/win/dug.<VERSION>.nupkg --api-key <CHOCOLATELY_API_KEY> --source https://push.chocolatey.org/`
