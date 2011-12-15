#!/bin/sh
kill `ps | grep $1 | awk '{print $1}'`
