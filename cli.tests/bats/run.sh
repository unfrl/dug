#!/bin/bash
set -e

# This is intended to be run from the root directory with `bats ./cli.tests/bats/run.sh`
# The publish command below is not used in CI/CD, its just there for running bats locally.
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
  run $DUG --output-format JSON google.com
  [ "$status" -eq 1 ]
  [[ "$output" == *"Error setting value to option 'output-format':"* ]]
}

@test "Invoking dug with invalid value in --continents should fail" {
  run $DUG --continents AZ,AS,NA google.com
  [ "$status" -eq 1 ]
  [[ "$output" == *"Error setting value to option 'continents':"* ]]
}

@test "Invoking dug with -v should enable verbose output" {
  run $DUG google.com -s 8.8.8.8 -v
  [ "$status" -eq 0 ]
  [[ "$output" == *"Verbose Output Enabled"* ]]
}

@test "Verify that the default run prints a table with A records" {
  run $DUG google.com -s 8.8.8.8
  [ "$status" -eq 0 ]
  # echo "ouput: $output"
  [[ "$output" =~ "A records for google.com" ]]
}

@test "Verify that requesting specific records prints the correct table" {
  run $DUG google.com -s 8.8.8.8 -q A,MX
  [ "$status" -eq 0 ]
  # echo "ouput: $output"
  [[ "$output" =~ "A,MX records for google.com" ]]
}

@test "Verify that requesting specific records, in any case, prints the correct table" {
  run $DUG google.com -s 8.8.8.8 -q A,mx,Ns
  [ "$status" -eq 0 ]
  # echo "ouput: $output"
  [[ "$output" =~ "A,MX,NS records for google.com" ]]
}