#!/bin/sh
# Writes wwwroot/posts/manifest.json and wwwroot/movies/manifest.json
cd "$(dirname "$0")/the-teenage-girl/wwwroot" || exit 1

for dir in posts movies; do
  out="$dir/manifest.json"
  printf '[' > "$out"
  first=1
  for f in "$dir"/*.md; do
    [ -e "$f" ] || continue
    [ "$first" -eq 1 ] || printf ',' >> "$out"
    printf '"%s"' "$(basename "$f" .md)" >> "$out"
    first=0
  done
  printf ']' >> "$out"
done