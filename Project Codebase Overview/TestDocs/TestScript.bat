
@ECHO OFF

ECHO ---------------

ECHO starttime: %TIME%

git log --diff-filter=A -- TextFile1.txt

ECHO endtime: %TIME%

PAUSE