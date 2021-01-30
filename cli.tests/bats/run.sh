#!./cli.tests/bats/libs/bats/bin/bats
# This test is designed for BATS. Ideally it should be able to be run from the root. If no build of dug is available it will build it then test it.
set -e

load 'libs/bats-support/load'
load 'libs/bats-assert/load'

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
@test "Verify that csv output doesnt have extra commas when using convenience template items like citycountryname and citycountrycontinentname" {
  run $DUG google.com --server-count 10 --output-format CSV --output-template ipaddress,citycountryname,citycountrycontinentname
  assert_success

  for i in "${lines[@]}"
  do
    :     
    line_comma_count=$(echo $i | grep -o ',' | wc -l)
    assert_equal $line_comma_count 2
  done
}

@test "Invoke 'dug version'" {
  run $DUG version
  assert_success
  assert_output "0.0.1" #The testing build should always be version 0.0.1
}

@test "Invoking dug without a Hostname should fail" {
  run $DUG
  assert_failure
  assert_line --index 0 "A Hostname must be provided. (run dug help or dug --help for more info)"
}

@test "Invoking dug with --output-format but without --output-template should fail" {
  run $DUG --output-format JSON google.com
  assert_failure
  assert_line --index 0 --partial "Error setting value to option 'output-format':"
}

@test "Invoking dug with invalid value in --continents should fail" {
  run $DUG --continents AZ,AS,NA google.com
  assert_failure
  assert_line --index 0 --partial "Error setting value to option 'continents':"
}

@test "Invoking dug with -v should enable verbose output" {
  run $DUG google.com -s 8.8.8.8 -v
  assert_success
  assert_line --index 0 --partial "Verbose Output Enabled"
}

@test "Verify that the default run prints a table with A records" {
  run $DUG google.com -s 8.8.8.8
  assert_success
  # echo "ouput: $output"
  assert_line --index 1 --partial "A records for google.com"
}

@test "Verify that requesting specific records prints the correct table" {
  run $DUG google.com -s 8.8.8.8 -q A,MX
  assert_success
  # echo "ouput: $output"
  assert_line --index 1 --partial "A,MX records for google.com"
}

@test "Verify that requesting specific records, provided in any casing, prints the correct table" {
  run $DUG google.com -s 8.8.8.8 -q A,mx,Ns
  assert_success
  # echo "ouput: $output"
  assert_line --partial "A,MX,NS records for google.com"
}

@test "Verify that specifying table-detail out of range fails" {
  run $DUG google.com -s 8.8.8.8 -q A -d 0
  assert_failure
  # echo "ouput: $output"
  assert_line --index 0 --partial "Error setting value to option 'd, table-detail':"
}
