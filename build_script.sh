#!/bin/sh

if [ ! -f "unity" ]; then
    echo "ERROR:"
    echo "No pointer to the unity-executable was found!"
    echo "Make a pointer to the unity executable in the folder of this script, and name it \"unity\""
    exit 1
fi

echo "Building environments..."
./unity -batchMode -quit -projectPath $(pwd) -executeMethod BuildScript.BuildAll -logFile local.log
echo "Results in $(pwd)/envs"
