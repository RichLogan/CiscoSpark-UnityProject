#! /bin/sh
project="unity-sdk-travis"

mkdir TravisBuild
cd TravisBuild
echo "Attempting to fetch Cisco Spark SDK"
mkdir -p Project/
git clone -b $TRAVIS_PULL_REQUEST_BRANCH --recursive https://github.com/RichLogan/CiscoSpark-UnitySDK.git Project
cd Project
echo "SDK Ready on branch:" $(git rev-parse --abbrev-ref HEAD)
