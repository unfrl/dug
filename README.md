# dug

A global DNS progagation checker that gives pretty output. Written in dotnet core

The **real** repository is located [here](https://git.kaijucode.com/matt/dug)

Notes:

Publish command for single binary with resource files: `dotnet publish -r linux-x64 -p:PublishSingleFile=true --self-contained true -o publish`

Dig command to test DNSSEC on a server: `dig @<dns_server_here> www.dnssec-tools.org +dnssec`
