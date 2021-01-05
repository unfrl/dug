---
id: templated_input
title: Templated Input
---

dug allows users to specify the format of server data provided to it.

## Default Server Format

When dug reads server data from `~/.dug/servers.csv` it expects it to have a certain structure. This is also the format that is expected when either `run` or `update` use the (-f, --file) option.

That structure is as follows:

`ip_address,country_code,city,dnssec,reliability`

Here is some more info on the fields:

| Field                 | Possible Values                                                                                                             |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| ip_address (required) | Any parseable IP (like `8.8.8.8`)                                                                                           |
| country_code          | ISO 3166-1 alpha country codes [(info)](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2#Officially_assigned_code_elements) |
| city                  | Anything                                                                                                                    |
| dnssec                | `true` or `false`                                                                                                           |
| reliability           | Any value in [0.0 - 1.0]                                                                                                    |

A valid file using the default format might look like:

```csv
ip_address,country_code,city,dnssec,reliability
2607:5300:203:1797::53,CA,,True,0.59
199.255.137.34,US,,False,0.7
82.146.26.2,BG,Sofia,True,1
```

## Custom Input Templating

The relevant options for templating input are `--data-columns`, `--data-headers-present`, and `--data-separator` and are present on both `run` and `update`. While their application on each verb is slightly different, their goal is the same: to tell dug the structure of the server data being provided to it. The notable difference is that on `update` they apply to servers provided via a file (-f) or a remote url (--update-url) whereas `run` doesnt have a remote url source specifier.

dug will allow the user to specify a file source (-f, --file) and will use it if it is in the default format. For example `dug run -f ~/.dug/servers.csv` will be fine. Once the user specifies a remote source (--update-url) or a file that is in another format, they will need to provide more information about the data.

More detailed descriptions of the input templating options can be found on the [run](./run) and [update](./update) verb pages, but heres some generalized info on them.

| Option                 | Description                                                                                                                                             | Possible Values                                                                                                               |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- | ------------- |
| --data-columns         | Specify the fields, and their order, of the server data. Applies to data from a file (-f)(`run` or `update`) or remotely (--update-url)(`update` only). | Any of the following (or multiple separated by commas): `ipaddress`, `countrycode`, `city`, `dnssec`, `reliability`, `ignore` |
| --data-headers-present | Specifies whether or not headers are present on the server data. Can only be used in conjuction with --data-columns                                     |
| --data-separator       | Specifies the separator to be used when parsing server data. Can only be used in conjuction with --data-columns.                                        | `,`                                                                                                                           | Any character |

### Custom Import Templating Example

Consider the following file named `custom_headers_server.csv`:

```csv
city,ip_address,country_code,dnssec,reliability,color
Rawalpindi,103.209.52.250,PK,False,0.85,red
Lake Saint Louis,8.8.8.8,US,True,1,blue
,8.8.4.4,US,True,1,
```

Its headers are as follows: `city,ipaddress,countrycode,dnssec,reliability,color`

Notice the last column, `color`. dug does not understand what a `color` field is, so this data should be ignored, that is the purpose of the `ignore` type in --data-columns

We could have dug understand, and use exclusively these servers for a run, with the following: `dug run -f ./custom_header_servers.csv --data-headers-present --data-columns city,ipaddress,countrycode,dnssec,reliability,ignore git.kaijucode.com`

Note that the above command tells us what data is in each column, and that the file's first line is headers and ought to be skipped.

If, for example, the above file used `;` instead of `,` as the column separator you could add `--data-separator ;` to the previous command and have it succeed.

### --data-columns Parseable Types

The fields specified in `--data-columns` must be parseable from the specified source for dug to use them. Any of the provided columns, __except ipaddress__, can be missing and will be ignored.

Here is more info on what values can live in what columns.

| Field       | Possible Values                                                                                                             |
| ----------- | --------------------------------------------------------------------------------------------------------------------------- |
| ipaddress   | Any parseable IP (like `8.8.8.8`)                                                                                           |
| countrycode | ISO 3166-1 alpha country codes [(info)](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2#Officially_assigned_code_elements) |
| city        | Anything                                                                                                                    |
| dnssec      | `true` or `1` or `false` or `0`                                                                                             |
| reliability | Any value in [0.0 - 1.0]                                                                                                    |
| ignore      | Anything, its not parsed                                                                                                    |
