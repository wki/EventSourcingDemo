#!/bin/bash
dir=`dirname $0`
mono $dir/.paket/paket.exe $*
