#!/usr/bin/env bash
# Classify Assets/TempleRun drift between EndlessRunnerTemplate and RunnerUGSTemplate.
#
# Usage (from either repo, in Git Bash):
#   bash .claude/skills/sync-templerun/compare.sh            # writes TSV to stdout
#   SIB=/d/elsewhere/EndlessRunnerTemplate bash compare.sh   # override checkout paths
#
# Output: TSV   status  sibling_last_commit  ugs_last_commit  changed_lines  path
#   identical     bytes match
#   eol-bom-only  differs only by CRLF/BOM (a checkout artifact - both repos store LF; ignore)
#   drift         real content difference (changed_lines = added+removed after normalizing)
#   drift-binary  binary file differs
#   only-sibling / only-ugs
#
# The comparison normalizes CRLF and a leading UTF-8 BOM, so working-tree line-ending
# differences (core.autocrlf varies per clone) never masquerade as drift.

SIB="${SIB:-/c/Repos/Github/EndlessRunnerTemplate}"
UGS="${UGS:-/c/Repos/Github/RunnerUGSTemplate}"
SUB="Assets/TempleRun"

if [ ! -d "$SIB/$SUB" ] || [ ! -d "$UGS/$SUB" ]; then
  echo "error: expected $SIB/$SUB and $UGS/$SUB to exist (set SIB= / UGS=)" >&2
  exit 1
fi

norm() { sed -e '1s/^\xEF\xBB\xBF//' -e 's/\r$//' "$1"; }

{ (cd "$SIB/$SUB" && find . -type f ! -name '*.meta' | sed 's|^\./||')
  (cd "$UGS/$SUB" && find . -type f ! -name '*.meta' | sed 's|^\./||'); } | sort -u | while IFS= read -r f; do
  a="$SIB/$SUB/$f"; b="$UGS/$SUB/$f"
  da() { git -C "$SIB" log -1 --format=%cs -- "$SUB/$f"; }
  db() { git -C "$UGS" log -1 --format=%cs -- "$SUB/$f"; }
  if [ ! -e "$a" ]; then printf 'only-ugs\t-\t%s\t-\t%s\n' "$(db)" "$f"
  elif [ ! -e "$b" ]; then printf 'only-sibling\t%s\t-\t-\t%s\n' "$(da)" "$f"
  elif cmp -s "$a" "$b"; then printf 'identical\t-\t-\t0\t%s\n' "$f"
  elif ! grep -qI . "$a"; then printf 'drift-binary\t%s\t%s\t-\t%s\n' "$(da)" "$(db)" "$f"
  elif cmp -s <(norm "$a") <(norm "$b"); then printf 'eol-bom-only\t-\t-\t0\t%s\n' "$f"
  else n=$(diff <(norm "$a") <(norm "$b") | grep -c '^[<>]'); printf 'drift\t%s\t%s\t%s\t%s\n' "$(da)" "$(db)" "$n" "$f"
  fi
done
