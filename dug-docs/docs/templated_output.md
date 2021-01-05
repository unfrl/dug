---
id: templated_output
title: Templated Output
---

While the output of dug's `run` verb is human-readable pretty tables, it is also capable of outputting CSV or JSON structured how the user wants. This functionality is controlled by `--output-template` and `--output-format`.

Heres some high-level information on them:

| Option            | Description                                                                                                         | Default  | Possible Values                                                                                                                                                                                                                                                                                   |
| ----------------- | ------------------------------------------------------------------------------------------------------------------- | -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| --output-template | Specify which data, and in what order, to put into out. Ignored if --output-format=`TABLES`.                        |          | Any of the following (or multiple separated by commas): `ipaddress`, `countrycode`, `city`, `dnssec`, `reliability`, `continentcode`, `countryname`, `countryflag`, `citycountryname`, `citycountrycontinentname`, `responsetime`, `recordtype`, `haserror`, `errormessage`, `errorcode`, `value` |
| --output-format   | Specify the output format. For formats other than the default you must also specify a template (--output-template). | `TABLES` | `TABLES`, `CSV`, `JSON`                                                                                                                                                                                                                                                                           |

`--output-format` is very simple and allows users to specify whether the want their output as JSON or CSV. If it is anything other than the default `TABLES` nothing else will be printed (progress animations, results, etc). If set to `CSV` the output will be a csv with the columns specified by `--output-template`. If it is set to `JSON` the output will be a JSON array containing objects whose fields are specified by `--output-template`.

## Templated Output Example

Templated output allows users to do really powerful stuff, usually by leveraging the capabilities of other tools.
For example, if you wanted to query the top 50 servers (by reliability) on each continent and determine which ones had errors, you could use dug combined with [jq](https://stedolan.github.io/jq/). The following command queries the top 50 servers per continent, gets the ouput as json, then asks jq to give us the ones that had an error:

`dug run --output-format JSON --output-template ipaddress,city,value,haserror --server-count 50 git.kaijucode.com | jq '.[] | select(.haserror==true)'`

The output should look something like:

```json
    {
        "ipaddress": "41.57.120.161",
        "city": "",
        "value": "ConnectionTimeout",
        "haserror": true
    }
    {
        "ipaddress": "45.124.53.159",
        "city": "Melbourne",
        "value": "ConnectionTimeout",
        "haserror": true
    }
    {
        "ipaddress": "154.73.165.189",
        "city": "",
        "value": "ConnectionTimeout",
        "haserror": true
    }
    ...
```

### --output-template Field Info

Heres some information on the fields that can be specified via `--output-template`:

| Field                    | Description & Reasonable Value                                                                                                                                                                                                                |
| ------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ipaddress                | The IPAddress of the queried server. Any parseable IP (like `8.8.8.8`)                                                                                                                                                                        |
| countrycode              | The country code of the queried server. ISO 3166-1 alpha country codes [(info)](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2#Officially_assigned_code_elements)                                                                           |
| city                     | The city of the queried server. Could be anything...                                                                                                                                                                                          |
| dnssec                   | Whether or not the queried server was know to have dnssec at the time of querying. `true` or `false`                                                                                                                                          |
| reliability              | The reliability of the queried server. Any value in [0.0 - 1.0] **Note:** This is before the reliability is updated with the result                                                                                                           |
| continentcode            | The ISO country code of the queried server.                                                                                                                                                                                                   |
| countryname              | The country name of the queried server. Could be anything...                                                                                                                                                                                  |
| countryflag              | The country's flag of the queried server as an emoji like: 🇵🇸 **Note:** Rendering support of flags is iffy in a lot of terminals/fonts/editors                                                                                                |
| citycountryname          | A pretty formatted string containing the city and country of the queried server. Like `Nahal Qatif, Palestine`. **NOTE:** This string contains commas, and breaks csv output [issue](https://github.com/unfrl/dug/issues/2)                   |
| citycountrycontinentname | A pretty formatted string containing the city, country, and continent of the queried server. like `Nahal Qatif, Palestine, Asia`. **NOTE:** this string contains commas, and breaks csv output [issue](https://github.com/unfrl/dug/issues/2) |
| responsetime             | The response time of the query (in ms).                                                                                                                                                                                                       |
| recordtype               | The record type of the response. Will be on of [these](./run#query-types)                                                                                                                                                                     |
| haserror                 | Whether or not the query resulted in an error. `true` or `false`                                                                                                                                                                              |
| errormessage             | The message associated with the error, it one exists                                                                                                                                                                                          |
| errorcode                | The error code associated with the error, almost never differs from errormessage                                                                                                                                                              |
| value                    | The value of the query response. **Note:** This varies dramatically depending on the query type (-q). Different records look very different                                                                                                   |
