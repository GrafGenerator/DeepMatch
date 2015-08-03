git commit -m "prepare to release $1"

git checkout master
git merge release/$1 --no-ff
git tag "$1"

git checkout develop
git merge release/$1 --no-ff

git branch -d release/$1
