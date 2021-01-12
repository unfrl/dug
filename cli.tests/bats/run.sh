#!/bin/bash

#This is intended to be run from the root directory with `bats ./cli.tests/bats/run.sh`
setup() {
  DUG=publish/dug

  if [ ! -f "$DUG" ]; then
    echo "dug executable doesnt exist, building..."
    dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -o publish ./cli
  fi
  if [ ! -f "$DUG" ]; then
    echo "Failed to build executable!"
    EXIT
  fi
}

@test "Invoke 'dug version'" {
  run $DUG version
  [ "$status" -eq 0 ]
  [ "$output" = "0.0.1" ]
}

@test "Invoking dug without a Hostname should fail" {
  run $DUG
  [ "$status" -eq 1 ]
  [ "$output" = "A Hostname must be provided. (run dug help or dug --help for more info)" ]
}

@test "Invoking dug with --output-format but without --output-template should fail" {
  run $DUG --output-format JSON
  [ "$status" -eq 1 ]
}
